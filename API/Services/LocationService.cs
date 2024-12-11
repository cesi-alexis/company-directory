using CompanyDirectory.API.Contexts;
using CompanyDirectory.API.Interfaces;
using CompanyDirectory.API.Utils;
using CompanyDirectory.Common;
using CompanyDirectory.Common.Exceptions;
using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace CompanyDirectory.API.Services
{
    /// <summary>
    /// Service pour gérer les opérations CRUD sur les localisations.
    /// </summary>
    public class LocationService(
        BusinessDirectoryDbContext context,
        IMemoryCache cache,
        IOptions<CacheSettings> cacheSettings) : ICrudService<Location>
    {
        private readonly BusinessDirectoryDbContext _context = context;
        private readonly IMemoryCache _cache = cache;
        private readonly CacheSettings _cacheSettings = cacheSettings.Value;

        /// <summary>
        /// Crée une nouvelle localisation.
        /// </summary>
        public async Task<Location> CreateAsync(Location location)
        {
            ValidateAsync(location);

            if (await IsDuplicateAsync(location.City))
                throw new ConflictException(string.Format(Messages.DuplicateName, location.City));

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            _cache.Remove("Locations_*");
            return location;
        }

        /// <summary>
        /// Récupère une liste paginée de localisations avec des champs dynamiques et des critères de recherche.
        /// </summary>
        /// <param name="searchTerm">Critère de recherche à appliquer sur le nom de la ville.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse, séparés par des virgules.</param>
        /// <param name="pageNumber">Numéro de la page.</param>
        /// <param name="pageSize">Nombre d'éléments par page.</param>
        /// <returns>Une liste paginée de modèles <see cref="LocationGetResponseViewModel"/>.</returns>
        public async Task<(IEnumerable<object>? Items, int TotalCount)> GetFilteredAsync(
            string? searchTerm = null,
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ValidationException(Messages.PaginationInvalid);

            var query = _context.Locations.AsQueryable();

            // Applique un filtre si un terme de recherche est fourni
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(l => EF.Functions.Like(l.City, $"%{searchTerm}%"));
            }

            // Applique un tri par défaut si nécessaire
            query = query.OrderBy(l => l.Id);

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(fields))
            {
                // Sélection dynamique avec System.Linq.Dynamic.Core
                var projectedQuery = query.Select($"new ({fields})");

                // Retourne les champs dynamiques
                var items = await projectedQuery.ToDynamicListAsync();
                return (items, totalCount);
            }

            // Projection complète si aucun champ spécifique n'est demandé
            var fullItems = await query.Skip((pageNumber - 1) * pageSize)
                                       .Take(pageSize)
                                       .Select(l => new LocationGetResponseViewModel
                                       {
                                           Id = l.Id,
                                           City = l.City
                                       }).ToListAsync();

            return (fullItems, totalCount);
        }


        /// <summary>
        /// Récupère une localisation spécifique par son identifiant avec des champs dynamiques.
        /// </summary>
        /// <param name="id">Identifiant unique de la localisation.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse, séparés par des virgules.</param>
        /// <returns>Un modèle <see cref="LocationGetResponseViewModel"/> avec les champs spécifiés ou null si non trouvé.</returns>
        public async Task<object?> GetAsync(int id, string? fields = null)
        {
            if (id <= 0)
                throw new ValidationException(Messages.InvalidId);

            var query = _context.Locations.Where(l => l.Id == id);

            if (!string.IsNullOrWhiteSpace(fields))
            {
                // Projection dynamique avec sélection des champs
                var projectedQuery = query.Select($"new CompanyDirectory.Models.ViewsModels.Responses.LocationGetResponseViewModel({fields})");
                return await projectedQuery.Cast<LocationGetResponseViewModel>().FirstOrDefaultAsync();
            }

            // Projection complète si aucun champ spécifique n'est demandé
            return await query.Select(l => new LocationGetResponseViewModel
            {
                Id = l.Id,
                City = l.City
            }).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Vérifie si une localisation existe par ID.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException(Messages.InvalidId);

            return await _context.Locations.AnyAsync(l => l.Id == id);
        }

        /// <summary>
        /// Vérifie si une localisation existe pour un nom donné.
        /// </summary>
        public async Task<bool> ExistsAsync(string city)
        {
            return await _context.Locations
                .AsNoTracking()
                .AnyAsync(s => s.City.Trim().ToLower() == city.Trim().ToLower());
        }

        /// <summary>
        /// Vérifie si un nom est en doublon pour une localisation.
        /// </summary>
        public async Task<bool> IsDuplicateAsync(string name, int? excludeId = null)
        {
            return await _context.Locations
                .AnyAsync(s => EF.Functions.Like(s.City.Trim().ToLower(), name.Trim().ToLower())
                               && (!excludeId.HasValue || s.Id != excludeId.Value));
        }

        /// <summary>
        /// Met à jour une localisation existante.
        /// </summary>
        public async Task<bool> UpdateAsync(int id, Location location)
        {
            if (id != location.Id)
                throw new ValidationException(Messages.IdMismatch);

            ValidateAsync(location);

            _context.Entry(location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                _cache.Remove($"Location_{id}");
                _cache.Remove("Locations_*");
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(id))
                    throw new NotFoundException(string.Format(Messages.LocationNotFound, id));
                throw;
            }
        }

        /// <summary>
        /// Supprime une localisation existante.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var location = await _context.Locations
                .Include(l => l.Workers)
                .FirstOrDefaultAsync(l => l.Id == id)
                ?? throw new NotFoundException(string.Format(Messages.LocationNotFound, id));

            if (location.Workers is not null && location.Workers.Count > 0)
                throw new ConflictException(Messages.LinkedWorkersConflict);

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            _cache.Remove($"Location_{id}");
            _cache.Remove("Locations_*");
            return true;
        }

        /// <summary>
        /// Valide les champs d'une localisation.
        /// </summary>
        public void ValidateAsync(Location location)
        {
            if (!Formats.IsValidName(location.City))
                throw new ValidationException(Messages.InvalidNameFormat);
        }
    }
}