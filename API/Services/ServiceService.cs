using Data.context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class ServiceService
    {
        private readonly BusinessDirectoryDbContext _context;

        public ServiceService(BusinessDirectoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Service>> GetAllServicesAsync()
        {
            return await _context.Services.ToListAsync();
        }

        public async Task<Service?> GetServiceByIdAsync(int id)
        {
            return await _context.Services.FindAsync(id);
        }

        public async Task<bool> UpdateServiceAsync(int id, Service service)
        {
            if (id != service.Id) return false;

            _context.Entry(service).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ServiceExistsAsync(id)) return false;
                throw;
            }
        }

        public async Task<Service> CreateServiceAsync(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task<bool> DeleteServiceAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return false;

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> ServiceExistsAsync(int id)
        {
            return await _context.Services.AnyAsync(s => s.Id == id);
        }
    }
}