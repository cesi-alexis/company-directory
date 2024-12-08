using API.Services;
using API.Utils;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        private readonly SiteService _siteService;

        public SiteController(SiteService siteService)
        {
            _siteService = siteService;
        }

        // GET: api/Site
        [HttpGet]
        public async Task<IActionResult> GetSites(
            [FromQuery] string? fields = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = Constants.MAX_PAGES)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var (sites, totalCount) = await _siteService.GetFilteredSitesAsync(fields, pageNumber, pageSize);
                return new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    Sites = sites
                };
            });
        }

        // GET: api/Site/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSite(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () => await _siteService.GetSiteByIdAsync(id));
        }

        // PUT: api/Site/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSite(int id, Site site)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _siteService.UpdateSiteAsync(id, site);
                return NoContent();
            });
        }

        // POST: api/Site
        [HttpPost]
        public async Task<IActionResult> PostSite(Site site)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var createdSite = await _siteService.CreateSiteAsync(site);
                return CreatedAtAction(nameof(GetSite), new { id = createdSite.Id }, createdSite);
            });
        }

        // DELETE: api/Site/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSite(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _siteService.DeleteSiteAsync(id);
                return NoContent();
            });
        }

        [HttpGet("exists/{city}")]
        public async Task<IActionResult> SiteExists(string city)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var exists = await _siteService.SiteExistsAsync(city);
                return new { Exists = exists };
            });
        }
    }
}
