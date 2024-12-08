using CompanyDirectory.Common;
using CompanyDirectory.Common.Exceptions;
using CompanyDirectory.API.Contexts;
using CompanyDirectory.API.Utils;
using CompanyDirectory.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CompanyDirectory.API.Services
{
    public class WorkerService(
        BusinessDirectoryDbContext context,
        IMemoryCache cache,
        IOptions<CacheSettings> cacheSettings,
        ServiceService serviceService,
        LocationService locationService)
    {
        private readonly BusinessDirectoryDbContext _context = context;
        private readonly IMemoryCache _cache = cache;
        private readonly CacheSettings _cacheSettings = cacheSettings.Value;
        private readonly ServiceService _serviceService = serviceService;
        private readonly LocationService _locationService = locationService;

        public async Task<(IEnumerable<object> Workers, int TotalCount)> GetFilteredWorkersAsync(
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ValidationException("Page number and page size must be greater than 0.");

            var cacheKey = $"Workers_{fields}_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out (IEnumerable<object> Workers, int TotalCount) cachedResult))
            {
                return cachedResult;
            }

            var query = _context.Workers.AsQueryable();
            var result = await ServiceUtils.GetFilteredAsync(query, fields, pageNumber, pageSize);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return result;
        }

        public async Task<Worker?> GetWorkerByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("The provided ID must be greater than 0.");

            var cacheKey = $"Worker_{id}";
            if (_cache.TryGetValue(cacheKey, out Worker? cachedWorker))
            {
                if (cachedWorker is not null)
                {
                    return cachedWorker;
                }
            }

            var worker = await _context.Workers
                .Include(e => e.Service)
                .Include(e => e.Location)
                .FirstOrDefaultAsync(e => e.Id == id) ?? throw new NotFoundException($"Worker with ID {id} was not found.");
            _cache.Set(cacheKey, worker, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return worker;
        }

        public async Task<bool> UpdateWorkerAsync(int id, Worker worker)
        {
            if (id != worker.Id)
                throw new ValidationException("The ID in the URL does not match the worker ID.");

            ValidateWorkerFields(worker);

            if (!await _serviceService.ServiceExistsAsync(worker.ServiceId))
                throw new NotFoundException($"Service with ID {worker.ServiceId} does not exist.");

            if (!await _locationService.LocationExistsAsync(worker.LocationId))
                throw new NotFoundException($"Location with ID {worker.LocationId} does not exist.");

            _context.Entry(worker).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _cache.Remove($"Worker_{id}");
                _cache.Remove($"Workers_*");
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await WorkerExistsAsync(id))
                    throw new NotFoundException($"Worker with ID {id} was not found.");
                throw;
            }
        }

        public async Task<Worker> CreateWorkerAsync(Worker worker)
        {
            ValidateWorkerFields(worker);

            if (await IsDuplicateEmailAsync(worker.Email))
            {
                throw new ConflictException($"An worker with the email '{worker.Email}' already exists.");
            }

            if (!await _serviceService.ServiceExistsAsync(worker.ServiceId))
            {
                throw new NotFoundException($"Service with ID {worker.ServiceId} does not exist.");
            }

            if (!await _locationService.LocationExistsAsync(worker.LocationId))
            {
                throw new NotFoundException($"Location with ID {worker.LocationId} does not exist.");
            }

            _context.Workers.Add(worker);
            await _context.SaveChangesAsync();

            _cache.Remove("Workers_*");
            return worker;
        }

        public async Task<bool> DeleteWorkerAsync(int id)
        {
            var worker = await _context.Workers.FindAsync(id) ?? throw new NotFoundException($"Worker with ID {id} was not found.");
            _context.Workers.Remove(worker);
            await _context.SaveChangesAsync();

            _cache.Remove($"Worker_{id}");
            _cache.Remove($"Workers_*");
            return true;
        }

        private async Task<bool> WorkerExistsAsync(int id)
        {
            return await _context.Workers.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> WorkerExistsAsync(string email)
        {
            // Utilisation d'EF.Functions.Like pour une comparaison insensible à la casse
            return await _context.Workers
                .AsNoTracking()
                .AnyAsync(l => EF.Functions.Like(l.Email, email));
        }

        private static void ValidateWorkerFields(Worker worker)
        {
            if (string.IsNullOrWhiteSpace(worker.FirstName))
            {
                throw new ValidationException($"The FirstName field is required. Provided value: '{worker.FirstName ?? "null"}'");
            }

            if (string.IsNullOrWhiteSpace(worker.LastName))
            {
                throw new ValidationException($"The LastName field is required. Provided value: '{worker.LastName ?? "null"}'");
            }

            if (!IsValidEmail(worker.Email))
            {
                throw new ValidationException($"Invalid email format. Provided value: '{worker.Email ?? "null"}'");
            }

            if (!IsValidPhoneNumber(worker.PhoneFixed))
            {
                throw new ValidationException($"Invalid PhoneFixed format. Provided value: '{worker.PhoneFixed ?? "null"}'. Must contain only digits.");
            }

            if (!IsValidPhoneNumber(worker.PhoneMobile))
            {
                throw new ValidationException($"Invalid PhoneMobile format. Provided value: '{worker.PhoneMobile ?? "null"}'. Must contain only digits.");
            }
        }

        private async Task<bool> IsDuplicateEmailAsync(string email, int? excludeId = null)
        {
            return await _context.Workers.AnyAsync(e =>
                e.Email == email && (!excludeId.HasValue || e.Id != excludeId.Value));
        }

        private static bool IsValidEmail(string email) =>
            !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Contains('.');

        private static bool IsValidPhoneNumber(string phoneNumber) =>
            !string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.All(char.IsDigit);
    }
}