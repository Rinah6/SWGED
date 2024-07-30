using API.Data.Entities;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/document_accesses")]
    [Authorize]
    public class DocumentAccessesController : ControllerBase
    {
        private readonly DocumentAccessesRepository _documentAccessesRepository;
        private readonly ProjectRepository _projectRepository;

        public DocumentAccessesController(DocumentAccessesRepository documentAccessesRepository, ProjectRepository projectRepository)
        {
            _documentAccessesRepository = documentAccessesRepository;
            _projectRepository = projectRepository;
        }

        [HttpGet("documents/{documentId}/accessors")]
        public async Task<IActionResult> GetDocumentAccessors(Guid documentId)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var currentUserProjectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (currentUserProjectId == null)
            {
                return StatusCode(403);
            }

            var documentAccessors = await _documentAccessesRepository.GetDocumentAccessors(documentId, (Guid)currentUserProjectId, currentUserId);

            return Ok(documentAccessors);
        }

        [HttpPost("documents/{documentId}/accessors")]
        public async Task<IActionResult> AddDocumentAccessors(Guid documentId, DocumentAccessorsToAdd documentAccessorsToAdd)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            for (int i = 0; i < documentAccessorsToAdd.UsersId.Count; i += 1)
            {
                await _documentAccessesRepository.AddDocumentAccessor(documentAccessorsToAdd.UsersId[i], documentId, currentUserId);
            }

            return Ok();
        }

        [HttpPatch("documents/{documentId}/accessors")]
        public async Task<IActionResult> AddDocumentAccessors(Guid documentId, DocumentAccessorsToDelete documentAccessorsToDelete)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            for (int i = 0; i < documentAccessorsToDelete.UsersId.Count; i += 1)
            {
                await _documentAccessesRepository.RemoveDocumentAccessor(documentAccessorsToDelete.UsersId[i], documentId, currentUserId);
            }

            return Ok();
        }

        [HttpGet("documents/{documentId}/can_be_accessed_by_anyone")]
        public async Task<IActionResult> CanTheDocumentBeAccessedByAnyone(Guid documentId)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var canTheDocumentBeAccessedByAnyone = await _documentAccessesRepository.CanTheDocumentBeAccessedByAnyone(documentId);

            return Ok(canTheDocumentBeAccessedByAnyone);
        }

        [HttpPatch("documents/{documentId}/can_be_accessed_by_anyone")]
        public async Task<IActionResult> SetCanBeAccessedByAnyone(Guid documentId, bool status)
        {
            await _documentAccessesRepository.SetCanBeAccessedByAnyone(documentId, status);

            return Ok();
        }
    }
}
