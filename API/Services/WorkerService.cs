using CompanyDirectory.API.Contexts;
using CompanyDirectory.API.Interfaces;
using CompanyDirectory.API.Utils;
using CompanyDirectory.Common;
using CompanyDirectory.Common.Exceptions;
using CompanyDirectory.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CompanyDirectory.API.Services
{
    /// <summary>
    /// Service pour gérer les opérations CRUD et le transfert d'employés.
    /// </summary>
    public class WorkerService(
        BusinessDirectoryDbContext context,
        IMemoryCache cache,
        IOptions<CacheSettings> cacheSettings,
        ICrudService<Service> serviceService,
        ICrudService<Location> locationService) : IWorkerService
    {
        private readonly BusinessDirectoryDbContext _context = context;
        private readonly IMemoryCache _cache = cache;
        private readonly CacheSettings _cacheSettings = cacheSettings.Value;
        private readonly ICrudService<Service> _serviceService = serviceService;
        private readonly ICrudService<Location> _locationService = locationService;

        /// <summary>
        /// Crée un nouvel employé.
        /// </summary>
        public async Task<Worker> CreateAsync(Worker worker)
        {
            ValidateAsync(worker);

            if (await IsDuplicateAsync(worker.Email))
                throw new ConflictException(string.Format(Messages.DuplicateEmail, worker.Email));

            if (!await _serviceService.ExistsAsync(worker.ServiceId))
                throw new NotFoundException(string.Format(Messages.ServiceNotFound, worker.ServiceId));

            if (!await _locationService.ExistsAsync(worker.LocationId))
                throw new NotFoundException(string.Format(Messages.LocationNotFound, worker.LocationId));

            _context.Workers.Add(worker);
            await _context.SaveChangesAsync();

            _cache.Remove("Workers_*");
            return worker;
        }

        /// <summary>
        /// Récupère une liste paginée d'employés avec des champs optionnels et un terme de recherche.
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
            var cacheKey = $"Workers_{searchTerm}_{fields}_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out (IEnumerable<object> Workers, int TotalCount) cachedResult))
            {
                return cachedResult;
            }

            // Préparation de la requête avec filtrage basé sur le terme de recherche
            var query = _context.Workers.AsQueryable();

            if (searchTerm != null)
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    throw new ValidationException(string.Format(Messages.InvalidSearchTerms, searchTerm ?? "null"));
                }

                query = query.Where(w => EF.Functions.Like(w.FirstName, $"%{searchTerm}%")
                                      || EF.Functions.Like(w.LastName, $"%{searchTerm}%")
                                      || EF.Functions.Like(w.PhoneFixed, $"%{searchTerm}%")
                                      || EF.Functions.Like(w.PhoneMobile, $"%{searchTerm}%")
                                      || EF.Functions.Like(w.Email, $"%{searchTerm}%"));
            }

            // Utilisation de ServiceUtils pour appliquer le filtrage et la pagination
            var result = await ServiceUtils.GetFilteredAsync(query, fields, pageNumber, pageSize);

            // Mise en cache des résultats pour améliorer les performances
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));

            return result;
        }

        /// <summary>
        /// Récupère un employé spécifique par son identifiant.
        /// </summary>
        public async Task<Worker?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException(Messages.InvalidId);

            var cacheKey = $"Worker_{id}";
            if (_cache.TryGetValue(cacheKey, out Worker? cachedWorker))
            {
                if (cachedWorker is not null)
                    return cachedWorker;
            }

            var worker = await _context.Workers
                .Include(e => e.Service)
                .Include(e => e.Location)
                .FirstOrDefaultAsync(e => e.Id == id)
                ?? throw new NotFoundException(string.Format(Messages.WorkerNotFound, id));

            _cache.Set(cacheKey, worker, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return worker;
        }

        /// <summary>
        /// Vérifie si un employé existe par ID.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Workers.AnyAsync(e => e.Id == id);
        }

        /// <summary>
        /// Vérifie si un employé existe pour un email donné.
        /// </summary>
        public async Task<bool> ExistsAsync(string email)
        {
            return await _context.Workers
                .AsNoTracking()
                .AnyAsync(s => s.Email.Trim().ToLower() == email.Trim().ToLower());
        }

        /// <summary>
        /// Vérifie si un email est en doublon pour un employé.
        /// </summary>
        public async Task<bool> IsDuplicateAsync(string email, int? excludeId = null)
        {
            return await _context.Workers.AnyAsync(e =>
                e.Email == email && (!excludeId.HasValue || e.Id != excludeId.Value));
        }

        /// <summary>
        /// Met à jour un employé existant.
        /// </summary>
        public async Task<bool> UpdateAsync(int id, Worker worker)
        {
            if (id != worker.Id)
                throw new ValidationException(Messages.IdMismatch);

            ValidateAsync(worker);

            if (!await _serviceService.ExistsAsync(worker.ServiceId))
                throw new NotFoundException(string.Format(Messages.ServiceNotFound, worker.ServiceId));

            if (!await _locationService.ExistsAsync(worker.LocationId))
                throw new NotFoundException(string.Format(Messages.LocationNotFound, worker.LocationId));

            _context.Entry(worker).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _cache.Remove($"Worker_{id}");
                _cache.Remove("Workers_*");
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(id))
                    throw new NotFoundException(string.Format(Messages.WorkerNotFound, id));
                throw;
            }
        }

        /// <summary>
        /// Supprime un employé existant.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var worker = await _context.Workers.FindAsync(id)
                ?? throw new NotFoundException(string.Format(Messages.WorkerNotFound, id));

            _context.Workers.Remove(worker);
            await _context.SaveChangesAsync();

            _cache.Remove($"Worker_{id}");
            _cache.Remove("Workers_*");
            return true;
        }

        /// <summary>
        /// Transfère un ou plusieurs employés vers une nouvelle localisation et/ou un nouveau service.
        /// </summary>
        public async Task<(int TotalWorkers, int SuccessCount, List<(int WorkerId, string ErrorMessage)> Errors)> TransferWorkersAsync(
            List<int> workerIds, int? newLocationId, int? newServiceId, bool allowPartialTransfer)
        {
            var totalWorkers = workerIds.Count;
            var successCount = 0;
            var errors = new List<(int WorkerId, string ErrorMessage)>();

            foreach (var workerId in workerIds)
            {
                try
                {
                    var worker = await GetByIdAsync(workerId) ?? throw new NotFoundException(string.Format(Messages.WorkerNotFound, workerId));
                    if (newLocationId.HasValue)
                    {
                        worker.LocationId = newLocationId.Value;
                    }

                    if (newServiceId.HasValue)
                    {
                        worker.ServiceId = newServiceId.Value;
                    }

                    ValidateAsync(worker);
                    await UpdateAsync(worker.Id, worker);

                    successCount++;
                }
                catch (Exception ex)
                {
                    if (!allowPartialTransfer)
                        throw;

                    errors.Add((WorkerId: workerId, ErrorMessage: ex.Message));
                }
            }

            return (totalWorkers, successCount, errors);
        }

        /// <summary>
        /// Valide les champs d'un employé.
        /// </summary>
        public void ValidateAsync(Worker worker)
        {
            if (!Formats.IsValidName(worker.FirstName))
                throw new ValidationException(string.Format(Messages.InvalidNameFormat, worker.FirstName));

            if (!Formats.IsValidName(worker.LastName))
                throw new ValidationException(string.Format(Messages.InvalidNameFormat, worker.LastName));

            if (!Formats.IsValidEmail(worker.Email))
                throw new ValidationException(Messages.InvalidEmailFormat);

            if (!Formats.IsValidPhoneNumber(worker.PhoneFixed))
                throw new ValidationException(Messages.InvalidPhoneNumberFormat);

            if (!Formats.IsValidPhoneNumber(worker.PhoneMobile))
                throw new ValidationException(Messages.InvalidPhoneNumberFormat);
        }
    }
}