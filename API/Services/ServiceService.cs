using CompanyDirectory.API.Contexts;
using CompanyDirectory.API.Interfaces;
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
    /// Service pour gérer les opérations CRUD sur les services.
    /// </summary>
    public class ServiceService(
        BusinessDirectoryDbContext context,
        IMemoryCache cache,
        IOptions<CacheSettings> cacheSettings) : ICrudService<Service>
    {
        private readonly BusinessDirectoryDbContext _context = context;
        private readonly IMemoryCache _cache = cache;
        private readonly CacheSettings _cacheSettings = cacheSettings.Value;

        /// <summary>
        /// Crée un nouveau service.
        /// </summary>
        public async Task<Service> CreateAsync(Service service)
        {
            ValidateAsync(service);

            if (await IsDuplicateAsync(service.Name))
                throw new ConflictException(string.Format(Messages.DuplicateName, service.Name));

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            _cache.Remove("Services_*");
            return service;
        }

        /// <summary>
        /// Récupère une liste paginée de services avec des champs dynamiques et des critères de recherche.
        /// </summary>
        /// <param name="searchTerm">Critère de recherche à appliquer sur le nom du service.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse, séparés par des virgules.</param>
        /// <param name="pageNumber">Numéro de la page.</param>
        /// <param name="pageSize">Nombre d'éléments par page.</param>
        /// <returns>Une liste paginée de modèles <see cref="ServiceGetResponseViewModel"/>.</returns>
        public async Task<(IEnumerable<object>? Items, int TotalCount)> GetFilteredAsync(
            string? searchTerm = null,
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ValidationException(Messages.PaginationInvalid);

            var query = _context.Services.AsQueryable();

            // Applique un filtre si un terme de recherche est fourni
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => EF.Functions.Like(s.Name, $"%{searchTerm}%"));
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
                                       .Select(s => new ServiceGetResponseViewModel
                                       {
                                           Id = s.Id,
                                           Name = s.Name
                                       }).ToListAsync();

            return (fullItems, totalCount);
        }

        /// <summary>
        /// Récupère un service spécifique par son identifiant avec des champs dynamiques.
        /// </summary>
        /// <param name="id">Identifiant unique du service.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse, séparés par des virgules.</param>
        /// <returns>Un modèle <see cref="ServiceGetResponseViewModel"/> avec les champs spécifiés ou null si non trouvé.</returns>
        public async Task<object?> GetAsync(int id, string? fields = null)
        {
            if (id <= 0)
                throw new ValidationException(Messages.InvalidId);

            var query = _context.Services.Where(s => s.Id == id);

            if (!string.IsNullOrWhiteSpace(fields))
            {
                // Projection dynamique avec sélection des champs
                var projectedQuery = query.Select($"new CompanyDirectory.Models.ViewsModels.Responses.ServiceGetResponseViewModel({fields})");
                return await projectedQuery.Cast<ServiceGetResponseViewModel>().FirstOrDefaultAsync();
            }

            // Projection complète si aucun champ spécifique n'est demandé
            return await query.Select(s => new ServiceGetResponseViewModel
            {
                Id = s.Id,
                Name = s.Name
            }).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Vérifie si un service existe par ID.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException(Messages.InvalidId);

            return await _context.Services.AnyAsync(s => s.Id == id);
        }

        /// <summary>
        /// Vérifie si un service existe pour un nom donné.
        /// </summary>
        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Services
                .AsNoTracking()
                .AnyAsync(s => s.Name.Trim().ToLower() == name.Trim().ToLower());
        }

        /// <summary>
        /// Vérifie si un nom est en doublon pour un service.
        /// </summary>
        public async Task<bool> IsDuplicateAsync(string name, int? excludeId = null)
        {
            return await _context.Services
                .AnyAsync(s => EF.Functions.Like(s.Name.Trim().ToLower(), name.Trim().ToLower())
                               && (!excludeId.HasValue || s.Id != excludeId.Value));
        }

        /// <summary>
        /// Met à jour un service existant.
        /// </summary>
        public async Task<bool> UpdateAsync(int id, Service service)
        {
            if (id != service.Id)
                throw new ValidationException(Messages.IdMismatch);

            ValidateAsync(service);

            if (await IsDuplicateAsync(service.Name, id))
                throw new ConflictException(string.Format(Messages.DuplicateName, service.Name));

            _context.Entry(service).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                _cache.Remove($"Service_{id}");
                _cache.Remove("Services_*");
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(id))
                    throw new NotFoundException(string.Format(Messages.ServiceNotFound, id));
                throw;
            }
        }

        /// <summary>
        /// Supprime un service existant.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var service = await _context.Services
                .Include(s => s.Workers)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NotFoundException(string.Format(Messages.ServiceNotFound, id));

            if (service.Workers is not null && service.Workers.Count > 0)
                throw new ConflictException(Messages.LinkedWorkersConflict);

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            _cache.Remove($"Service_{id}");
            _cache.Remove("Services_*");
            return true;
        }

        /// <summary>
        /// Valide les champs d'un service.
        /// </summary>
        public void ValidateAsync(Service service)
        {
            if (!Formats.IsValidName(service.Name))
                throw new ValidationException(Messages.InvalidNameFormat);
        }
    }
}