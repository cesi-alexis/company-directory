using API.Services;
using API.Utils;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ServiceController : ControllerBase
{
    private readonly ServiceService _serviceService;

    public ServiceController(ServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetServices([FromQuery] string? fields = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = Constants.MAX_PAGES)
    {
        return await ResponseUtils.HandleResponseAsync(async () =>
        {
            var (services, totalCount) = await _serviceService.GetFilteredServicesAsync(fields, pageNumber, pageSize);
            return new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Services = services
            };
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetService(int id)
    {
        return await ResponseUtils.HandleResponseAsync(async () => await _serviceService.GetServiceByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> PostService(Service service)
    {
        return await ResponseUtils.HandleResponseAsync(async () =>
        {
            var createdService = await _serviceService.CreateServiceAsync(service);
            return CreatedAtAction(nameof(GetService), new { id = createdService.Id }, createdService);
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutService(int id, Service service)
    {
        return await ResponseUtils.HandleResponseAsync(async () =>
        {
            await _serviceService.UpdateServiceAsync(id, service);
            return NoContent();
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        return await ResponseUtils.HandleResponseAsync(async () =>
        {
            await _serviceService.DeleteServiceAsync(id);
            return NoContent();
        });
    }

    [HttpGet("exists/{name}")]
    public async Task<IActionResult> ServiceExists(string name)
    {
        return await ResponseUtils.HandleResponseAsync(async () =>
        {
            var exists = await _serviceService.ServiceExistsAsync(name);
            return new { Exists = exists };
        });
    }
}
