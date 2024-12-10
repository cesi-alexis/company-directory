using CompanyDirectory.Common;
using CompanyDirectory.Common.Exceptions;
using CompanyDirectory.API.Contexts;
using CompanyDirectory.API.Utils;
using CompanyDirectory.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using CompanyDirectory.API.Interfaces;

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
        /// Récupère une liste paginée de services avec des champs optionnels et un terme de recherche.
        /// </summary>
        public async Task<(IEnumerable<object>? Items, int TotalCount)> GetFilteredAsync(
            string? searchTerm = null,
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES)
        {
            // Validation des paramètres de pagination
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ValidationException(Messages.PaginationInvalid);

            // Génération de la clé de cache basée sur les paramètres
            var cacheKey = $"Services_{searchTerm}_{fields}_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out (IEnumerable<object> Services, int TotalCount) cachedResult))
            {
                return cachedResult;
            }

            // Préparation de la requête avec filtrage basé sur le terme de recherche
            var query = _context.Services.AsQueryable();

            if (searchTerm != null)
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    throw new ValidationException(string.Format(Messages.InvalidSearchTerms, searchTerm ?? "null"));
                }

                query = query.Where(s => EF.Functions.Like(s.Name, $"%{searchTerm}%"));
            }

            // Utilisation de ServiceUtils pour appliquer le filtrage et la pagination
            var result = await ServiceUtils.GetFilteredAsync(query, fields, pageNumber, pageSize);

            // Mise en cache des résultats pour améliorer les performances
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));

            return result;
        }

        /// <summary>
        /// Récupère un service spécifique par son identifiant.
        /// </summary>
        public async Task<Service?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException(Messages.InvalidId);

            var cacheKey = $"Service_{id}";
            if (_cache.TryGetValue(cacheKey, out Service? cachedService))
            {
                if (cachedService is not null)
                    return cachedService;
            }

            var service = await _context.Services
                .Include(s => s.Workers) // Inclut les employés liés
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NotFoundException(string.Format(Messages.ServiceNotFound, id));

            _cache.Set(cacheKey, service, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return service;
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