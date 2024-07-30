using Microsoft.AspNetCore.Mvc;
using API.Data.Entities;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("api/tom_pro_db_connections")]
    [ApiController]
    [Authorize]
    public class TomProConnectionController : ControllerBase
    {
        private readonly ProjectRepository _projectRepository;
        private readonly TomProConnectionRepository _tomProConnectionRepository;

        public TomProConnectionController(ProjectRepository projectRepository, TomProConnectionRepository TomProConnectionRepository)
        {
            _projectRepository = projectRepository;
            _tomProConnectionRepository = TomProConnectionRepository;
        }

        [HttpPost("databases")]
        public async Task<ActionResult> GetDatabases(DBConnexionDetails dBConnexionDetails)
        {
            try
            {
                var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

                var databases = await _tomProConnectionRepository.GetDatabases(dBConnexionDetails);

                return Ok(databases);
            }
            catch (Exception)
            {
                return StatusCode(403);
            }
        }

        [HttpPost("new")]
        public async Task<ActionResult> PostNewDBConnection(TomProConnectionToAdd tomProConnectionToAdd)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            await _tomProConnectionRepository.PostTomProConnection(tomProConnectionToAdd, (Guid)projectId, currentUserId);

            return Ok();
        }

        [HttpGet("/api/tom_pro_connections")]
        public async Task<ActionResult> GetTomProConnections()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            var tomProConnections = await _tomProConnectionRepository.GetTomProConnections((Guid)projectId);

            return Ok(tomProConnections);
        }

        [HttpGet("/api/tom_pro_connections/{tomProConnectionId}/databases")]
        public async Task<ActionResult> GetTomProDatabases(Guid tomProConnectionId)
        {
            var tomProConnections = await _tomProConnectionRepository.GetTomProDatabases(tomProConnectionId);

            return Ok(tomProConnections);
        }

        [HttpPatch("databases/project")]
        public async Task<ActionResult> UpdateTomProDBConnection(Tomate_DB_ConnectionToUpdate tomate_DB_ConnectionToUpdate)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            await _tomProConnectionRepository.UpdateTomProDBConnection((Guid)projectId, tomate_DB_ConnectionToUpdate);

            return Ok();
        }

        [HttpPost("/api/tom_pro/liquidations")]
        public async Task<IActionResult> GetLiquidations(LiquidationSearchOption liquidationSearchOption)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            var tomProDBConnection = await _tomProConnectionRepository.GetTomProDBConnection(liquidationSearchOption.TomProConnectionId, liquidationSearchOption.TomProDatabaseId);

            if (tomProDBConnection == null)
            {
                return StatusCode(403);
            }

            var liquidations = await _tomProConnectionRepository.GetLiquidations(tomProDBConnection, liquidationSearchOption.Code);

            return Ok(liquidations);
        }

        [HttpGet("/api/tom_pro/avances")]
        public async Task<IActionResult> GetAvances(AvanceSearchOption avanceSearchOption)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            var tomProDBConnection = await _tomProConnectionRepository.GetTomProDBConnection(avanceSearchOption.TomProConnectionId, avanceSearchOption.TomProDatabaseId);

            if (tomProDBConnection == null)
            {
                return StatusCode(403);
            }

            var avances = await _tomProConnectionRepository.GetAvances(tomProDBConnection, avanceSearchOption.Code);

            return Ok(avances);
        }

        [HttpGet("/api/tom_pro/justificatifs")]
        public async Task<IActionResult> GetJustificatifs(JustificatifSearchOption justificatifSearchOption)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            var tomProDBConnection = await _tomProConnectionRepository.GetTomProDBConnection(justificatifSearchOption.TomProConnectionId, justificatifSearchOption.TomProDatabaseId);

            if (tomProDBConnection == null)
            {
                return StatusCode(403);
            }

            var justificatifs = await _tomProConnectionRepository.GetJustificatifs(tomProDBConnection, justificatifSearchOption.Code);

            return Ok(justificatifs);
        }

        [HttpGet("/api/tom_pro/reversements")]
        public async Task<IActionResult> GetReversements(ReversementSearchOption reversementSearchOption)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            var tomProDBConnection = await _tomProConnectionRepository.GetTomProDBConnection(reversementSearchOption.TomProConnectionId, reversementSearchOption.TomProDatabaseId);

            if (tomProDBConnection == null)
            {
                return StatusCode(403);
            }

            var reversements = await _tomProConnectionRepository.GetReversements(tomProDBConnection, reversementSearchOption.Code);

            return Ok(reversements);
        }

        [HttpPatch("/api/tom_pro/liquidations")]
        public async Task<ActionResult> UpdateLiquidationLink(LiquidationToUpdate liquidationToUpdate)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            var tomProDBConnection = await _tomProConnectionRepository.GetTomProDBConnection(liquidationToUpdate.TomProConnectionId, liquidationToUpdate.TomProDatabaseId);

            if (tomProDBConnection == null)
            {
                return StatusCode(403);
            }

            await _tomProConnectionRepository.UpdateLiquidationLink(tomProDBConnection, liquidationToUpdate);

            return Ok();
        }

        [HttpPatch("/api/tom_pro/avances")]
        public async Task<ActionResult> UpdateAvanceLink(AvanceToUpdate avanceToUpdate)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            var tomProDBConnection = await _tomProConnectionRepository.GetTomProDBConnection(avanceToUpdate.TomProConnectionId, avanceToUpdate.TomProDatabaseId);

            if (tomProDBConnection == null)
            {
                return StatusCode(403);
            }

            await _tomProConnectionRepository.UpdateAvanceLink(tomProDBConnection, avanceToUpdate);

            return Ok();
        }

        [HttpPatch("/api/tom_pro/justificatifs")]
        public async Task<ActionResult> UpdateJustificatifLink(JustificatifToUpdate justificatifToUpdate)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            var tomProDBConnection = await _tomProConnectionRepository.GetTomProDBConnection(justificatifToUpdate.TomProConnectionId, justificatifToUpdate.TomProDatabaseId);

            if (tomProDBConnection == null)
            {
                return StatusCode(403);
            }

            await _tomProConnectionRepository.UpdateJustificatifLink(tomProDBConnection, justificatifToUpdate);

            return Ok();
        }

        [HttpPatch("/api/tom_pro/reversements")]
        public async Task<ActionResult> UpdateReversementLink(ReversementToUpdate reversementToUpdate)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            var tomProDBConnection = await _tomProConnectionRepository.GetTomProDBConnection(reversementToUpdate.TomProConnectionId, reversementToUpdate.TomProDatabaseId);

            if (tomProDBConnection == null)
            {
                return StatusCode(403);
            }

            await _tomProConnectionRepository.UpdateReversementLink(tomProDBConnection, reversementToUpdate);

            return Ok();
        }
    }
}
