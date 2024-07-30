using Microsoft.AspNetCore.Mvc;
using API.Data;
using Microsoft.AspNetCore.Authorization;
using API.Repositories;
using API.Data.Entities;

namespace API.Controllers
{
    [Route("api/sites")]
    [ApiController]
    [Authorize]
    public class SiteController : ControllerBase
    {
        private readonly SiteRepository _siteRepository;

        public SiteController(SiteRepository siteRepository)
        {
            _siteRepository = siteRepository;

        }

        [HttpPost("")]
        public async Task<IActionResult> PostSite(SiteToAdd siteToAdd)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _siteRepository.AddNewSite(siteToAdd, currentUserId);

            return Ok();
        }


        [HttpGet("")]
        public async Task<ActionResult> GetSites()
        {

            var sites = await _siteRepository.GetAll();

            return Ok(sites);
        }

        [HttpGet("/api/sites/{siteId}")]
        public async Task<ActionResult> GetSiteInfo(Guid siteId)
        {
            var siteInfo = await _siteRepository.GetSiteDetails(siteId);

            if (siteInfo == null)
            {
                return NotFound();
            }

            return Ok(new SiteToUpdate
            {
                SiteId = siteInfo.SiteId,
                Name = siteInfo.Name,

            });
        }

        [HttpDelete("{siteId}")]
        public async Task<IActionResult> DeleteSite(Guid siteId)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _siteRepository.Delete(siteId, currentUserId);

            return Ok();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(Guid id, SiteToUpdate siteToUpdate)
        {
            await _siteRepository.Update(id, siteToUpdate);

            return Ok();
        }

    }
}
