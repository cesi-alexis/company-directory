using CompanyDirectory.Common;
using CompanyDirectory.API.Services;
using CompanyDirectory.API.Utils;
using CompanyDirectory.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CompanyDirectory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController(LocationService locationService) : ControllerBase
    {
        private readonly LocationService _locationService = locationService;

        // GET: api/Location
        [HttpGet]
        public async Task<IActionResult> GetLocations(
            [FromQuery] string? fields = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = Constants.MAX_PAGES)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var (locations, totalCount) = await _locationService.GetFilteredLocationsAsync(fields, pageNumber, pageSize);
                return new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    Locations = locations
                };
            });
        }

        // GET: api/Location/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocation(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () => await _locationService.GetLocationByIdAsync(id));
        }

        // PUT: api/Location/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, Location location)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _locationService.UpdateLocationAsync(id, location);
                return NoContent();
            });
        }

        // POST: api/Location
        [HttpPost]
        public async Task<IActionResult> PostLocation(Location location)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var createdLocation = await _locationService.CreateLocationAsync(location);
                return CreatedAtAction(nameof(GetLocation), new { id = createdLocation.Id }, createdLocation);
            });
        }

        // DELETE: api/Location/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _locationService.DeleteLocationAsync(id);
                return NoContent();
            });
        }

        [HttpGet("exists/{city}")]
        public async Task<IActionResult> LocationExists(string city)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var exists = await _locationService.LocationExistsAsync(city);
                return new { Exists = exists };
            });
        }
    }
}
