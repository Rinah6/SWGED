using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using API.Dto;
using API.Data.Entities;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using API.Context;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/projects")]
    [ApiController]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly SoftGED_DBContext _db;

        private readonly IMapper _mapper;
        private readonly ProjectRepository _projectRepository;
        private readonly ProjectDocumentsReceiverRepository _projectDocumentsReceiverRepository;

        public ProjectController(SoftGED_DBContext db, IConfiguration configuration, IMapper mapper, ProjectRepository projectRepository, ProjectDocumentsReceiverRepository projectDocumentsReceiverRepository)
        {
            _db = db;
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
            _mapper = mapper;
            _projectRepository = projectRepository;
            _projectDocumentsReceiverRepository = projectDocumentsReceiverRepository;
        }

        [HttpGet("")]
        public async Task<ActionResult> Get()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var projects = await _projectRepository.GetAll();

            return Ok(projects);
        }

        [HttpGet("/api/projects/soa")]
        public async Task<ActionResult> GetPS(string soa)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var soas = await _db.Soas.FirstOrDefaultAsync(project => project.Name == soa && project.DeletionDate == null);

            var projects = await _projectRepository.GetAllBySoaId(soas.Id);

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(Guid id)
        {
            return Ok(_mapper.Map<ProjectDto?>(await _projectRepository.Get(id)));
        }

        [HttpPost("")]
        public async Task<ActionResult> ProjectToAdd(ProjectToAdd projectToAdd)
        {
            if (_db.Projects.Any(project => project.Name == projectToAdd.Name && project.DeletionDate == null))
                return BadRequest("Le projet existe déjà!");

            //int soaID = _db.Soas.FirstOrDefault(a => a.Name == projectToAdd.SOA).Id;

            int soaID = 1;

            await _projectRepository.AddNewProject(projectToAdd, soaID);

            return Ok();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(Guid id, ProjectToUpdate projectToUpdate)
        {
            await _projectRepository.Update(id, projectToUpdate);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _projectRepository.Delete(id);

            return Ok();
        }

        [HttpGet("{projectId}/users")]
        public async Task<ActionResult> GetUsersByProjectId(Guid projectId)
        {
            var users = await _projectRepository.GetUsersByProjectId(projectId);

            return Ok(users);
        }

        [HttpGet("users")]
        public async Task<ActionResult> GetUsersByProjectId()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            var users = await _projectRepository.GetUsersByProjectId(project.Id);

            return Ok(users);
        }

        [HttpGet("/api/projects/{projectId}/documents_receivers")]
        public async Task<IActionResult> GetProjectDocumentsReceivers(Guid projectId)
        {
            var projectDocumentReceivers = await _projectDocumentsReceiverRepository.GetProjectDocumentsReceivers(projectId);

            return Ok(projectDocumentReceivers);
        }

        [HttpGet("/api/projects/{projectId}/not-documents_receivers")]
        public async Task<IActionResult> GetProjectNotDocumentsReceivers(Guid projectId)
        {
            var projectDocumentReceivers = await _projectDocumentsReceiverRepository.GetProjectDocumentsReceivers(projectId, false);

            return Ok(projectDocumentReceivers);
        }

        [HttpPost("/api/projects/{projectId}/documents_receivers")]
        public async Task<IActionResult> PostProjectNotDocumentsReceivers(Guid projectId, ProjectDocumentsReceiversToAdd projectDocumentsReceiversToAdd)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            if (project.Id != projectId)
            {
                return StatusCode(403);
            }

            await _projectDocumentsReceiverRepository.PostProjectDocumentsReceivers(projectDocumentsReceiversToAdd.UsersId);

            return Ok();
        }

        [HttpDelete("/api/projects/documents_receivers/{userId}")]
        public async Task<IActionResult> DeleteProjectDocumentReceiver(Guid userId)
        {
            await _projectDocumentsReceiverRepository.SetIsDocumentsReceiver(userId, false);

            return Ok();
        }
    }
}
