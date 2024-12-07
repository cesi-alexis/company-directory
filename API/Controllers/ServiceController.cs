using API.Services;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly ServiceService _businessService;

        public ServiceController(ServiceService businessService)
        {
            _businessService = businessService;
        }

        // GET: api/Service
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            var services = await _businessService.GetAllServicesAsync();
            return Ok(services);
        }

        // GET: api/Service/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            var service = await _businessService.GetServiceByIdAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            return Ok(service);
        }

        // PUT: api/Service/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutService(int id, Service service)
        {
            var result = await _businessService.UpdateServiceAsync(id, service);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Service
        [HttpPost]
        public async Task<ActionResult<Service>> PostService(Service service)
        {
            var createdService = await _businessService.CreateServiceAsync(service);
            return CreatedAtAction("GetService", new { id = createdService.Id }, createdService);
        }

        // DELETE: api/Service/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var result = await _businessService.DeleteServiceAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}