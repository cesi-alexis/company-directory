using Data.context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class SiteService
    {
        private readonly BusinessDirectoryDbContext _context;

        public SiteService(BusinessDirectoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Site>> GetAllSitesAsync()
        {
            return await _context.Sites.ToListAsync();
        }

        public async Task<Site?> GetSiteByIdAsync(int id)
        {
            return await _context.Sites.FindAsync(id);
        }

        public async Task<bool> UpdateSiteAsync(int id, Site site)
        {
            if (id != site.Id) return false;

            _context.Entry(site).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SiteExistsAsync(id)) return false;
                throw;
            }
        }

        public async Task<Site> CreateSiteAsync(Site site)
        {
            _context.Sites.Add(site);
            await _context.SaveChangesAsync();
            return site;
        }

        public async Task<bool> DeleteSiteAsync(int id)
        {
            var site = await _context.Sites.FindAsync(id);
            if (site == null) return false;

            _context.Sites.Remove(site);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> SiteExistsAsync(int id)
        {
            return await _context.Sites.AnyAsync(s => s.Id == id);
        }
    }
}