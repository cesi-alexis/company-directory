using Data.context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class SiteService
    {
        private readonly BusinessDirectoryDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _cacheSettings;

        public SiteService(
            BusinessDirectoryDbContext context,
            IMemoryCache cache,
            IOptions<CacheSettings> cacheSettings)
        {
            _context = context;
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task<(IEnumerable<object> Sites, int TotalCount)> GetFilteredSitesAsync(
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ValidationException("Page number and page size must be greater than 0.");

            var cacheKey = $"Sites_{fields}_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out (IEnumerable<object> Sites, int TotalCount) cachedResult))
            {
                return cachedResult;
            }

            var query = _context.Sites.AsQueryable();
            var result = await ServiceUtils.GetFilteredAsync(query, fields, pageNumber, pageSize);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return result;
        }

        public async Task<Site> GetSiteByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("The provided ID must be greater than 0.");

            var cacheKey = $"Site_{id}";
            if (_cache.TryGetValue(cacheKey, out Site cachedSite))
            {
                return cachedSite;
            }

            var site = await _context.Sites
                .Include(s => s.Employees) // Include related data if necessary
                .FirstOrDefaultAsync(s => s.Id == id);

            if (site == null)
                throw new NotFoundException($"Site with ID {id} was not found.");

            _cache.Set(cacheKey, site, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return site;
        }

        public async Task<Site> CreateSiteAsync(Site site)
        {
            ValidateSiteFields(site);

            if (await IsCityDuplicateAsync(site.City))
                throw new ConflictException($"A site with the city '{site.City}' already exists.");

            _context.Sites.Add(site);
            await _context.SaveChangesAsync();

            _cache.Remove("Sites_*");
            return site;
        }

        public async Task<bool> UpdateSiteAsync(int id, Site site)
        {
            if (id != site.Id)
                throw new ValidationException("The ID in the URL does not match the site ID.");

            ValidateSiteFields(site);

            if (await IsCityDuplicateAsync(site.City, id))
                throw new ConflictException($"A site with the city '{site.City}' already exists.");

            _context.Entry(site).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Invalidate cache
                _cache.Remove($"Site_{id}");
                _cache.Remove("AllSites");
                _cache.Remove("Sites_*");

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SiteExistsAsync(id))
                    throw new NotFoundException($"Site with ID {id} was not found.");
                throw;
            }
        }

        public async Task<bool> DeleteSiteAsync(int id)
        {
            var site = await _context.Sites.Include(s => s.Employees).FirstOrDefaultAsync(s => s.Id == id);
            if (site == null)
                throw new NotFoundException($"Site with ID {id} was not found.");

            if (site.Employees != null && site.Employees.Any())
                throw new ConflictException("Cannot delete a site that has linked employees.");

            _context.Sites.Remove(site);
            await _context.SaveChangesAsync();

            // Invalidate cache
            _cache.Remove($"Site_{id}");
            _cache.Remove("AllSites");
            _cache.Remove("Sites_*");

            return true;
        }

        public async Task<bool> SiteExistsAsync(int id)
        {
            return await _context.Sites.AnyAsync(s => s.Id == id);
        }

        public async Task<bool> SiteExistsAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ValidationException("City name must be provided.");

            return await _context.Sites.AnyAsync(s => s.City.ToLower() == city.ToLower());
        }

        private async Task<bool> IsCityDuplicateAsync(string city, int? excludeId = null)
        {
            return await _context.Sites.AnyAsync(s =>
                s.City.ToLower().Trim() == city.ToLower().Trim() && (!excludeId.HasValue || s.Id != excludeId.Value));
        }

        private async Task<bool> HasLinkedEmployeesAsync(int siteId)
        {
            return await _context.Employees.AnyAsync(e => e.SiteId == siteId);
        }

        private void ValidateSiteFields(Site site)
        {
            if (string.IsNullOrWhiteSpace(site.City))
            {
                throw new ValidationException($"The City field is required. Provided value: '{site.City ?? "null"}'");
            }
        }
    }
}