using API.Data.Entities;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/document_types")]
    [Authorize]
    public class DocumentTypesController : Controller
    {
        private readonly DocumentTypeRepository _documentTypeRepository;
        private readonly ProjectRepository _projectRepository;

        public DocumentTypesController(DocumentTypeRepository documentTypeRepository, ProjectRepository projectRepository)
        {
            _documentTypeRepository = documentTypeRepository;
            _projectRepository = projectRepository;
        }

        [HttpPost("")]
        public async Task<IActionResult> PostDocumentType(DocumentTypeToAdd documentTypeToAdd)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            await _documentTypeRepository.PostDocumentType(documentTypeToAdd, project.Id, currentUserId);

            return Ok();
        }

        [HttpGet("")]
        public async Task<IActionResult> GetDocumentTypes()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            var documentTypes = await _documentTypeRepository.GetDocumentTypesByProjectId(project.Id);

            return Ok(documentTypes);
        }

        [HttpGet("{documentTypeId}")]
        public async Task<IActionResult> GetDocumentTypeDetails(Guid documentTypeId)
        {
            var documentTypeDetails = await _documentTypeRepository.GetDocumentTypeDetails(documentTypeId);

            return Ok(documentTypeDetails);
        }

        [HttpPatch("{documentTypeId}/title")]
        public async Task<IActionResult> UpdateDocumentTypeTitle(Guid documentTypeId, DocumentTypeTitleToUpdate documentTypeTitleToUpdate)
        {
            await _documentTypeRepository.UpdateDocumentTypeTitle(documentTypeId, documentTypeTitleToUpdate.Title, documentTypeTitleToUpdate.Sites);

            return Ok();
        }

        [HttpPatch("steps/{documentTypeStepId}/details")]
        public async Task<IActionResult> UpdateDocumentTypeStep(Guid documentTypeStepId, DocumentTypeStepToUpdate documentTypeStepToUpdate)
        {
            await _documentTypeRepository.UpdateDocumentTypeStep(documentTypeStepId, documentTypeStepToUpdate);

            return Ok();
        }

        [HttpPost("{documentTypeId}/steps")]
        public async Task<IActionResult> AddDocumentTypeStep(Guid documentTypeId, DocumentTypeStepsToAdd documentTypeStepsToAdd)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            for (int i = 0; i < documentTypeStepsToAdd.Steps.Count; i += 1)
            {
                var stepId = await _documentTypeRepository.PostDocumentTypeStep(documentTypeStepsToAdd.Steps[i], documentTypeId, currentUserId);

                for (int j = 0; j < documentTypeStepsToAdd.Steps[i].UsersId.Count; j += 1)
                {
                    await _documentTypeRepository.PostDocumentTypeUserStep(documentTypeStepsToAdd.Steps[i].UsersId[j], stepId, currentUserId);
                }
            }

            return Ok();
        }

        [HttpPatch("steps")]
        public async Task<IActionResult> DeleteDocumentTypeStep(DocumentTypeStepsToDelete documentTypeStepsToDelete)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            for (int i = 0; i < documentTypeStepsToDelete.StepsId.Count; i += 1)
            {
                await _documentTypeRepository.DeleteDocumentTypeStep(documentTypeStepsToDelete.StepsId[i], currentUserId);
            }

            return Ok();
        }

        [HttpPost("steps/{documentTypeStepId}/validators")]
        public async Task<IActionResult> AddValidators(Guid documentTypeStepId, DocumentStepValidatorsToAdd documentStepValidatorsToAdd)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            for (int i = 0; i < documentStepValidatorsToAdd.ValidatorsId.Count; i += 1)
            {
                await _documentTypeRepository.PostDocumentTypeUserStep(documentStepValidatorsToAdd.ValidatorsId[i], documentTypeStepId, currentUserId);
            }

            return Ok();
        }

        [HttpPatch("steps/{documentTypeStepId}/validators")]
        public async Task<IActionResult> DeleteValidators(Guid documentTypeStepId, DocumentStepValidatorsToAdd documentStepValidatorsToAdd)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            for (int i = 0; i < documentStepValidatorsToAdd.ValidatorsId.Count; i += 1)
            {
                await _documentTypeRepository.DeleteDocumentTypeUserStep(documentStepValidatorsToAdd.ValidatorsId[i], documentTypeStepId, currentUserId);
            }

            return Ok();
        }

        [HttpDelete("{documentTypeId}")]
        public async Task<IActionResult> DeleteDocumentType(Guid documentTypeId)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _documentTypeRepository.DeleteDocumentType(documentTypeId, currentUserId);

            return Ok();
        }
    }
}
