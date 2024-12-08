using CompanyDirectory.Contexts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class LocationService
    {
        private readonly BusinessDirectoryDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _cacheSettings;

        public LocationService(
            BusinessDirectoryDbContext context,
            IMemoryCache cache,
            IOptions<CacheSettings> cacheSettings)
        {
            _context = context;
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task<(IEnumerable<object> Locations, int TotalCount)> GetFilteredLocationsAsync(
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ValidationException("Page number and page size must be greater than 0.");

            var cacheKey = $"Locations_{fields}_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out (IEnumerable<object> Locations, int TotalCount) cachedResult))
            {
                return cachedResult;
            }

            var query = _context.Locations.AsQueryable();
            var result = await ServiceUtils.GetFilteredAsync(query, fields, pageNumber, pageSize);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return result;
        }

        public async Task<Location> GetLocationByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("The provided ID must be greater than 0.");

            var cacheKey = $"Location_{id}";
            if (_cache.TryGetValue(cacheKey, out Location cachedLocation))
            {
                return cachedLocation;
            }

            var location = await _context.Locations
                .Include(s => s.Workers) // Include related data if necessary
                .FirstOrDefaultAsync(s => s.Id == id);

            if (location == null)
                throw new NotFoundException($"Location with ID {id} was not found.");

            _cache.Set(cacheKey, location, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return location;
        }

        public async Task<Location> CreateLocationAsync(Location location)
        {
            ValidateLocationFields(location);

            if (await IsCityDuplicateAsync(location.City))
                throw new ConflictException($"A location with the city '{location.City}' already exists.");

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            _cache.Remove("Locations_*");
            return location;
        }

        public async Task<bool> UpdateLocationAsync(int id, Location location)
        {
            if (id != location.Id)
                throw new ValidationException("The ID in the URL does not match the location ID.");

            ValidateLocationFields(location);

            if (await IsCityDuplicateAsync(location.City, id))
                throw new ConflictException($"A location with the city '{location.City}' already exists.");

            _context.Entry(location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Invalidate cache
                _cache.Remove($"Location_{id}");
                _cache.Remove("AllLocations");
                _cache.Remove("Locations_*");

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await LocationExistsAsync(id))
                    throw new NotFoundException($"Location with ID {id} was not found.");
                throw;
            }
        }

        public async Task<bool> DeleteLocationAsync(int id)
        {
            var location = await _context.Locations.Include(s => s.Workers).FirstOrDefaultAsync(s => s.Id == id);
            if (location == null)
                throw new NotFoundException($"Location with ID {id} was not found.");

            if (location.Workers != null && location.Workers.Any())
                throw new ConflictException("Cannot delete a location that has linked workers.");

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            // Invalidate cache
            _cache.Remove($"Location_{id}");
            _cache.Remove("AllLocations");
            _cache.Remove("Locations_*");

            return true;
        }

        public async Task<bool> LocationExistsAsync(int id)
        {
            return await _context.Locations.AnyAsync(s => s.Id == id);
        }

        public async Task<bool> LocationExistsAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ValidationException("City name must be provided.");

            return await _context.Locations.AnyAsync(s => s.City.ToLower() == city.ToLower());
        }

        private async Task<bool> IsCityDuplicateAsync(string city, int? excludeId = null)
        {
            return await _context.Locations.AnyAsync(s =>
                s.City.ToLower().Trim() == city.ToLower().Trim() && (!excludeId.HasValue || s.Id != excludeId.Value));
        }

        private async Task<bool> HasLinkedWorkersAsync(int locationId)
        {
            return await _context.Workers.AnyAsync(e => e.LocationId == locationId);
        }

        private void ValidateLocationFields(Location location)
        {
            if (string.IsNullOrWhiteSpace(location.City))
            {
                throw new ValidationException($"The City field is required. Provided value: '{location.City ?? "null"}'");
            }
        }
    }
}