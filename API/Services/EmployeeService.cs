using Data.context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class EmployeeService
    {
        private readonly BusinessDirectoryDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _cacheSettings;
        private readonly ServiceService _serviceService;
        private readonly SiteService _siteService;

        public EmployeeService(
            BusinessDirectoryDbContext context,
            IMemoryCache cache,
            IOptions<CacheSettings> cacheSettings,
            ServiceService serviceService,
            SiteService siteService)
        {
            _context = context;
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
            _serviceService = serviceService;
            _siteService = siteService;
        }

        public async Task<(IEnumerable<object> Employees, int TotalCount)> GetFilteredEmployeesAsync(
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ValidationException("Page number and page size must be greater than 0.");

            var cacheKey = $"Employees_{fields}_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out (IEnumerable<object> Employees, int TotalCount) cachedResult))
            {
                return cachedResult;
            }

            var query = _context.Employees.AsQueryable();
            var result = await ServiceUtils.GetFilteredAsync(query, fields, pageNumber, pageSize);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return result;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("The provided ID must be greater than 0.");

            var cacheKey = $"Employee_{id}";
            if (_cache.TryGetValue(cacheKey, out Employee cachedEmployee))
            {
                return cachedEmployee;
            }

            var employee = await _context.Employees
                .Include(e => e.Service)
                .Include(e => e.Site)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                throw new NotFoundException($"Employee with ID {id} was not found.");

            _cache.Set(cacheKey, employee, TimeSpan.FromMinutes(_cacheSettings.DefaultCacheDuration));
            return employee;
        }

        public async Task<bool> UpdateEmployeeAsync(int id, Employee employee)
        {
            if (id != employee.Id)
                throw new ValidationException("The ID in the URL does not match the employee ID.");

            ValidateEmployeeFields(employee);

            if (!await _serviceService.ServiceExistsAsync(employee.ServiceId))
                throw new NotFoundException($"Service with ID {employee.ServiceId} does not exist.");

            if (!await _siteService.SiteExistsAsync(employee.SiteId))
                throw new NotFoundException($"Site with ID {employee.SiteId} does not exist.");

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _cache.Remove($"Employee_{id}");
                _cache.Remove($"Employees_*");
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EmployeeExistsAsync(id))
                    throw new NotFoundException($"Employee with ID {id} was not found.");
                throw;
            }
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            ValidateEmployeeFields(employee);

            if (await IsDuplicateEmailAsync(employee.Email))
            {
                throw new ConflictException($"An employee with the email '{employee.Email}' already exists.");
            }

            if (!await _serviceService.ServiceExistsAsync(employee.ServiceId))
            {
                throw new NotFoundException($"Service with ID {employee.ServiceId} does not exist.");
            }

            if (!await _siteService.SiteExistsAsync(employee.SiteId))
            {
                throw new NotFoundException($"Site with ID {employee.SiteId} does not exist.");
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            _cache.Remove("Employees_*");
            return employee;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                throw new NotFoundException($"Employee with ID {id} was not found.");

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            _cache.Remove($"Employee_{id}");
            _cache.Remove($"Employees_*");
            return true;
        }

        private async Task<bool> EmployeeExistsAsync(int id)
        {
            return await _context.Employees.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> EmployeeExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("Email must be provided.");

            return await _context.Employees.AnyAsync(e => e.Email.ToLower() == email.ToLower());
        }

        private void ValidateEmployeeFields(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.FirstName))
            {
                throw new ValidationException($"The FirstName field is required. Provided value: '{employee.FirstName ?? "null"}'");
            }

            if (string.IsNullOrWhiteSpace(employee.LastName))
            {
                throw new ValidationException($"The LastName field is required. Provided value: '{employee.LastName ?? "null"}'");
            }

            if (!IsValidEmail(employee.Email))
            {
                throw new ValidationException($"Invalid email format. Provided value: '{employee.Email ?? "null"}'");
            }

            if (!IsValidPhoneNumber(employee.PhoneFixed))
            {
                throw new ValidationException($"Invalid PhoneFixed format. Provided value: '{employee.PhoneFixed ?? "null"}'. Must contain only digits.");
            }

            if (!IsValidPhoneNumber(employee.PhoneMobile))
            {
                throw new ValidationException($"Invalid PhoneMobile format. Provided value: '{employee.PhoneMobile ?? "null"}'. Must contain only digits.");
            }
        }

        private async Task<bool> IsDuplicateEmailAsync(string email, int? excludeId = null)
        {
            return await _context.Employees.AnyAsync(e =>
                e.Email == email && (!excludeId.HasValue || e.Id != excludeId.Value));
        }

        private bool IsValidEmail(string email) =>
            !string.IsNullOrWhiteSpace(email) && email.Contains("@") && email.Contains(".");

        private bool IsValidPhoneNumber(string phoneNumber) =>
            !string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.All(char.IsDigit);
    }
}