using API.Services;
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
        public async Task<ActionResult<IEnumerable<Site>>> GetSites()
        {
            var sites = await _siteService.GetAllSitesAsync();
            return Ok(sites);
        }

        // GET: api/Site/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Site>> GetSite(int id)
        {
            var site = await _siteService.GetSiteByIdAsync(id);

            if (site == null)
            {
                return NotFound();
            }

            return Ok(site);
        }

        // PUT: api/Site/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSite(int id, Site site)
        {
            var result = await _siteService.UpdateSiteAsync(id, site);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Site
        [HttpPost]
        public async Task<ActionResult<Site>> PostSite(Site site)
        {
            var createdSite = await _siteService.CreateSiteAsync(site);
            return CreatedAtAction("GetSite", new { id = createdSite.Id }, createdSite);
        }

        // DELETE: api/Site/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSite(int id)
        {
            var result = await _siteService.DeleteSiteAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}