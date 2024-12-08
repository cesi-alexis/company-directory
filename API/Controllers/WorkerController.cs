using CompanyDirectory.API.Common;
using CompanyDirectory.API.Services;
using CompanyDirectory.API.Utils;
using CompanyDirectory.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CompanyDirectory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkersController(WorkerService workerService) : ControllerBase
    {
        private readonly WorkerService _workerService = workerService;

        [HttpGet]
        public async Task<IActionResult> GetWorkers([FromQuery] string? fields = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = Constants.MAX_PAGES)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var (workers, totalCount) = await _workerService.GetFilteredWorkersAsync(fields, pageNumber, pageSize);
                return new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    Workers = workers
                };
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorker(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () => await _workerService.GetWorkerByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> PostWorker(Worker worker)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var createdWorker = await _workerService.CreateWorkerAsync(worker);
                return CreatedAtAction(nameof(GetWorker), new { id = createdWorker.Id }, createdWorker);
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutWorker(int id, Worker worker)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _workerService.UpdateWorkerAsync(id, worker);
                return NoContent();
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _workerService.DeleteWorkerAsync(id);
                return NoContent();
            });
        }

        [HttpGet("exists/{email}")]
        public async Task<IActionResult> WorkerExists(string email)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var exists = await _workerService.WorkerExistsAsync(email);
                return new { Exists = exists };
            });
        }
    }
}