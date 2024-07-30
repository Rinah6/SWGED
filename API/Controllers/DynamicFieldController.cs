using API.Data.Entities;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/dynamic_fields")]
    [ApiController]
    [Authorize]
    public class DynamicFieldController : ControllerBase
    {
        private readonly DynamicFieldRepository _dynamicFieldRepository;
        private readonly ProjectRepository _projectRepository;

        public DynamicFieldController(
            DynamicFieldRepository dynamicFieldRepository,
            ProjectRepository projectRepository
        )
        {
            _dynamicFieldRepository = dynamicFieldRepository;
            _projectRepository = projectRepository;
        }

        [HttpGet("global/types")]
        public async Task<IActionResult> GetGlobalDynamicFieldTypes()
        {
            return Ok(await _dynamicFieldRepository.GetDynamicFieldTypes());
        }

        [HttpDelete("global/{dynamicFieldItemId}/types")]
        public async Task<IActionResult> RemoveDynamicFieldItem(Guid dynamicFieldItemId)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _dynamicFieldRepository.RemoveDynamicFieldItem(dynamicFieldItemId, currentUserId);

            return Ok();
        }

        [HttpGet("global/list")]
        public async Task<IActionResult> GetGlobalDynamicFieldsListByProjectId()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            return Ok(await _dynamicFieldRepository.GetGlobalDynamicFieldsListByProjectId(project.Id));
        }

        [HttpGet("global")]
        public async Task<IActionResult> GetAllGlobalDynamicFieldsByProjectId()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            return Ok(await _dynamicFieldRepository.GetAllGlobalDynamicFieldsByProjectId(project.Id));
        }

        [HttpGet("global/suppliers/{projectId}")]
        [Authorize(AuthenticationSchemes = "jfien434YUGfbjjr94")]
        public async Task<IActionResult> GetAllGlobalDynamicFieldsByProjectId(Guid projectId)
        {
            var project = await _projectRepository.Get(projectId);

            if (project == null)
            {
                return StatusCode(403);
            }

            return Ok(await _dynamicFieldRepository.GetAllGlobalSuppliersDynamicFieldsByProjectId(project.Id));
        }

        [HttpGet("global/{Id}")]
        public async Task<IActionResult> GetGlobalDynamicDetailByIdAndProjectId(Guid Id)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            var res = await _dynamicFieldRepository.GetGlobalDynamicDetailByIdAndProjectId(Id, project.Id);

            if (res == null)
            {
                return NotFound();
            }

            return Ok(res);
        }

        [HttpPost("global")]
        public async Task<IActionResult> AddGlobalDynamicField(GlobalDynamicFieldToAdd globalDynamicFieldToAdd)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            await _dynamicFieldRepository.AddGlobalDynamicField(globalDynamicFieldToAdd, project.Id, currentUserId);

            return Ok();
        }

        [HttpPost("global/items")]
        public async Task<IActionResult> AddDynamicFieldItem(DynamicFieldItemToAdd dynamicFieldItemToAdd)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _dynamicFieldRepository.AddDynamicFieldItem(dynamicFieldItemToAdd.Value, dynamicFieldItemToAdd.DynamicFieldId, currentUserId);

            return Ok();
        }

        [HttpDelete("global/{dynamicFieldId}")]
        public async Task<IActionResult> RemoveGlobalDynamicField(Guid dynamicFieldId)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _dynamicFieldRepository.RemoveGlobalDynamicField(dynamicFieldId, currentUserId);

            return Ok();
        }

        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllDynamicFieldsByDocumentId(Guid documentId)
        {
            return Ok(await _dynamicFieldRepository.GetAllDynamicFieldsByDocumentId(documentId));
        }

        [HttpPatch("global/{dynamicFieldId}/title")]
        public async Task<IActionResult> UpdateDynamicFieldLabel(Guid dynamicFieldId, DynamicFieldLabelToUpdate dynamicFieldLabelToUpdate)
        {
            await _dynamicFieldRepository.UpdateDynamicFieldLabel(dynamicFieldId, dynamicFieldLabelToUpdate.Label);

            return Ok();
        }

        [HttpPatch("global/{dynamicFieldId}/requirement")]
        public async Task<IActionResult> UpdateDynamicFieldRequirement(Guid dynamicFieldId, DynamicFieldRequirementToUpdate dynamicFieldRequirementToUpdate)
        {
            await _dynamicFieldRepository.UpdateDynamicFieldRequirement(dynamicFieldId, dynamicFieldRequirementToUpdate.IsRequired);

            return Ok();
        }

        [HttpPatch("global/{dynamicFieldId}/visibility/users-project")]
        public async Task<IActionResult> UpdateDynamicFieldUsersProjectVisibility(Guid dynamicFieldId, DynamicFieldUsersProjectVisibilityToUpdate dynamicFieldUsersProjectVisibilityToUpdate)
        {
            await _dynamicFieldRepository.UpdateDynamicFieldUsersProjectVisibility(dynamicFieldId, dynamicFieldUsersProjectVisibilityToUpdate.IsForUsersProject);

            return Ok();
        }

        [HttpPatch("global/{dynamicFieldId}/visibility/suppliers")]
        public async Task<IActionResult> UpdateDynamicFieldSuppliersVisibility(Guid dynamicFieldId, DynamicFieldSuppliersVisibilityToUpdate dynamicFieldSuppliersVisibilityToUpdate)
        {
            await _dynamicFieldRepository.UpdateDynamicFieldSuppliersVisibility(dynamicFieldId, dynamicFieldSuppliersVisibilityToUpdate.IsForSuppliers);

            return Ok();
        }

        [HttpPost("/api/dynamic_attachements/documents/{documentId}")]
        public async Task<IActionResult> AddDynamicAttachementsToDocument(Guid documentId, Guid globalDynamicFieldId, [FromForm] DynamicAttachementToAddToDocument dynamicAttachementToAddToDocument)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _dynamicFieldRepository.AddDocumentDynamicAttachement(currentUserId, dynamicAttachementToAddToDocument.File, documentId, globalDynamicFieldId);

            return Ok();
        }

        [HttpGet("/api/dynamic_attachements/documents/{documentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDocumentDynamicAttachements(Guid documentId)
        {
            var dynamicAttachements = await _dynamicFieldRepository.GetDocumentDynamicAttachements(documentId);

            return Ok(dynamicAttachements);
        }

        [HttpGet("/api/download/dynamic_attachements/{dynamicFieldId}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadDynamicAttachement(Guid dynamicFieldId, Guid documentId)
        {
            var dynamicAttachementPath = await _dynamicFieldRepository.GetDynamicAttachementPath(dynamicFieldId, documentId);

            if (dynamicAttachementPath == null)
            {
                return NotFound();
            }

            var fileStream = new FileStream(Path.Combine("wwwroot", "store", dynamicAttachementPath), FileMode.Open, FileAccess.Read);

            var contentType = "application/octet-stream";

            return File(fileStream, contentType, "");
        }
    }
}
