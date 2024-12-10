using CompanyDirectory.Common;
using CompanyDirectory.Common.Exceptions;
using CompanyDirectory.API.Contexts;
using CompanyDirectory.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using CompanyDirectory.API.Utils;
using CompanyDirectory.API.Interfaces;

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
        /// Récupère une liste paginée de localisations avec des champs optionnels et un terme de recherche.
        /// </summary>
        public async Task<(IEnumerable<Object>? Items, int TotalCount)> GetFilteredAsync(
            string? searchTerm = null,
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES)
        {
            // Validation des paramètres de pagination
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ValidationException(Messages.PaginationInvalid);

            // Génération de la clé de cache basée sur les paramètres
            var cacheKey = $"Locations_{searchTerm}_{fields}_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out (IEnumerable<object> Locations, int TotalCount) cachedResult))
            {
                return cachedResult;
            }

            // Préparation de la requête avec filtrage basé sur le terme de recherche
            var query = _context.Locations.AsQueryable();

            if (searchTerm != null)
            {
                if(string.IsNullOrWhiteSpace(searchTerm))
                {
                    throw new ValidationException(string.Format(Messages.InvalidSearchTerms, searchTerm));
                }

                query = query.Where(l => EF.Functions.Like(l.City, $"%{searchTerm}%"));
            }

            // Utilisation de ServiceUtils pour appliquer le filtrage et la pagination
            var result = await ServiceUtils.GetFilteredAsync(query, fields, pageNumber, pageSize);

            // Mise en cache des résultats pour améliorer les performances
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));

            return result;
        }

        /// <summary>
        /// Récupère une localisation spécifique par son identifiant.
        /// </summary>
        public async Task<Location?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException(Messages.InvalidId);

            var cacheKey = $"Location_{id}";
            if (_cache.TryGetValue(cacheKey, out Location? cachedLocation))
            {
                if (cachedLocation is not null)
                    return cachedLocation;
            }

            var location = await _context.Locations
                .Include(l => l.Workers)
                .FirstOrDefaultAsync(l => l.Id == id)
                ?? throw new NotFoundException(string.Format(Messages.LocationNotFound, id));

            _cache.Set(cacheKey, location, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return location;
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