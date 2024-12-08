using Data.context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class ServiceService
    {
        private readonly BusinessDirectoryDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _cacheSettings;

        public ServiceService(
            BusinessDirectoryDbContext context,
            IMemoryCache cache,
            IOptions<CacheSettings> cacheSettings)
        {
            _context = context;
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task<(IEnumerable<object> Services, int TotalCount)> GetFilteredServicesAsync(
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ValidationException("Page number and page size must be greater than 0.");

            var cacheKey = $"Services_{fields}_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out (IEnumerable<object> Services, int TotalCount) cachedResult))
            {
                return cachedResult;
            }

            var query = _context.Services.AsQueryable();
            var result = await ServiceUtils.GetFilteredAsync(query, fields, pageNumber, pageSize);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return result;
        }

        public async Task<Service> GetServiceByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("The provided ID must be greater than 0.");

            var cacheKey = $"Service_{id}";
            if (_cache.TryGetValue(cacheKey, out Service cachedService))
            {
                return cachedService;
            }

            var service = await _context.Services
                .Include(s => s.Employees) // Include related data if necessary
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
                throw new NotFoundException($"Service with ID {id} was not found.");

            _cache.Set(cacheKey, service, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return service;
        }

        public async Task<Service> CreateServiceAsync(Service service)
        {
            ValidateServiceFields(service);

            if (await IsDuplicateNameAsync(service.Name))
            {
                throw new ConflictException($"A service with the name '{service.Name}' already exists.");
            }

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            _cache.Remove("Services_*");
            return service;
        }

        public async Task<bool> UpdateServiceAsync(int id, Service service)
        {
            if (id != service.Id)
                throw new ValidationException("The ID in the URL does not match the service ID.");

            ValidateServiceFields(service);

            if (await IsDuplicateNameAsync(service.Name, id))
                throw new ConflictException("A service with the same name already exists.");

            _context.Entry(service).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Invalidate cache for updated service
                _cache.Remove($"Service_{id}");
                _cache.Remove("AllServices");
                _cache.Remove("Services_*");
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ServiceExistsAsync(id))
                    throw new NotFoundException($"Service with ID {id} was not found.");
                throw;
            }
        }

        public async Task<bool> DeleteServiceAsync(int id)
        {
            var service = await _context.Services.Include(s => s.Employees).FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
                throw new NotFoundException($"Service with ID {id} was not found.");

            if (service.Employees != null && service.Employees.Any())
                throw new ConflictException("Cannot delete a service that has linked employees.");

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            // Invalidate cache for deleted service
            _cache.Remove($"Service_{id}");
            _cache.Remove("AllServices");
            _cache.Remove("Services_*");
            return true;
        }

        public async Task<bool> ServiceExistsAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("The provided Service ID must be greater than 0.");

            return await _context.Services.AnyAsync(s => s.Id == id);
        }

        public async Task<bool> ServiceExistsAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Service name must be provided.");

            return await _context.Services.AnyAsync(s => s.Name.ToLower() == name.ToLower());
        }

        private void ValidateServiceFields(Service service)
        {
            if (string.IsNullOrWhiteSpace(service.Name))
            {
                throw new ValidationException($"The Name field is required. Provided value: '{service.Name ?? "null"}'");
            }
        }

        private async Task<bool> IsDuplicateNameAsync(string name, int? excludeId = null)
        {
            return await _context.Services
                .AnyAsync(s => EF.Functions.Like(s.Name.Trim().ToLower(), name.Trim().ToLower())
                               && (!excludeId.HasValue || s.Id != excludeId.Value));
        }
    }
}