using API.Data;
using API.Data.Entities;
using API.Data.Entities.Dto;
using API.Dto;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("")]
    public class DocumentsProcessesController : ControllerBase
    {
        private readonly DocumentRepository _documentRepository;
        private readonly DocumentsProcessesRepository _documentsProcessesRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly DocumentService _documentService;
        private readonly DynamicFieldRepository _dynamicFieldRepository;

        public DocumentsProcessesController(
            DocumentRepository documentRepository,
            DocumentsProcessesRepository documentsProcessesRepository,
            ProjectRepository projectRepository,
            DocumentService documentService,
            DynamicFieldRepository dynamicFieldRepository
        )
        {
            _documentRepository = documentRepository;
            _documentsProcessesRepository = documentsProcessesRepository;
            _projectRepository = projectRepository;
            _documentService = documentService;
            _dynamicFieldRepository = dynamicFieldRepository;
        }

        [HttpGet("/api/users_documents_roles")]
        public async Task<ActionResult> GetUserDocumentRoles()
        {
            var roles = await _documentsProcessesRepository.GetUserDocumentRoles();

            return Ok(roles);
        }

        [HttpPost("/api/supplier_documents/{documentId}/validation_circuit")]
        public async Task<IActionResult> PostValidationCircuit(Guid documentId, ValidationCircuitToAdd validationCircuitToAdd)
        {
            Model.Document? document = await _documentRepository.Get(documentId);

            if (document == null)
            {
                return NotFound();
            }

            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _documentsProcessesRepository.UpdateDocumentStatus(documentId, DocumentStatus.Ongoing);

            await _documentsProcessesRepository.AddToSuppliersDocumentsSendings(documentId, currentUserId);

            for (int i = 0; i < validationCircuitToAdd.UsersSteps.Count; i += 1)
            {
                await _documentsProcessesRepository.AddDocumentValidatorStep(validationCircuitToAdd.UsersSteps[i], documentId);
            }

            return Ok();
        }

        [HttpPost("/api/documents/new_validation_circuit")]
        public async Task<ActionResult> AddNewDocumentValidationCircuit([FromForm] DocumentValidationCircuitToAdd documentValidationCircuitToAdd)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            var recipients = JsonConvert.DeserializeObject<List<RecipientsStep>>(documentValidationCircuitToAdd.Recipients);

            var document = await _documentService.CreateDocument(new NewDocumentDetails
            {
                DocumentFile = documentValidationCircuitToAdd.DocumentFile,
                Title = documentValidationCircuitToAdd.Title,
                Attachements = documentValidationCircuitToAdd.Attachements,
                Object = documentValidationCircuitToAdd.Object,
                Message = documentValidationCircuitToAdd.Message,
                RSF = Convert.ToBoolean(documentValidationCircuitToAdd.RSF),
                Site = documentValidationCircuitToAdd.Site,
            }, currentUserId, DocumentStatus.Ongoing, documentValidationCircuitToAdd.Site);

            if (recipients != null)
            {
                for (int i = 0; i < recipients.Count; i += 1)
                {
                    await _documentsProcessesRepository.AddDocumentStep(recipients[i], document.Id);
                }
            }

            var globalDynamicFields = JsonConvert.DeserializeObject<List<GlobalDynamicFieldDto>>(documentValidationCircuitToAdd.GlobalDynamicFields)!;

            for (int i = 0; i < globalDynamicFields.Count; i += 1)
            {
                await _dynamicFieldRepository.AddDocumentDynamicField(document.Id, globalDynamicFields[i].Id, globalDynamicFields[i].Value);
            }

            return Ok(document.Id);
        }

        [HttpPost("/api/documents/archive")]
        public async Task<ActionResult> ArchiveDocument([FromForm] DocumentToArchive documentToArchive)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            var document = await _documentService.CreateDocument(documentToArchive.File, documentToArchive.Attachements, documentToArchive.Title, documentToArchive.Object, documentToArchive.Message, Convert.ToBoolean(documentToArchive.RSF), currentUserId, DocumentStatus.Archived, documentToArchive.Site);

            if (document == null)
            {
                return StatusCode(403);
            }

            var globalDynamicFields = JsonConvert.DeserializeObject<List<GlobalDynamicFieldDto>>(documentToArchive.GlobalDynamicFields)!;

            for (int i = 0; i < globalDynamicFields.Count; i += 1)
            {
                await _dynamicFieldRepository.AddDocumentDynamicField(document.Id, globalDynamicFields[i].Id, globalDynamicFields[i].Value);
            }

            return Ok(document.Id);
        }

        [HttpPost("/api/documents/{documentId}/validate")]
        public async Task<ActionResult> Validate(Guid documentId, DocumentDto commentaire)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _documentsProcessesRepository.ValidateDocument(documentId, currentUserId, commentaire.Message);

            return Ok();
        }

        [HttpPost("/api/documents/{documentId}/cancel")]
        public async Task<ActionResult> CancelDocument(Guid documentId, DocumentToDeny documentToDeny)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _documentsProcessesRepository.CancelDocument(documentId, currentUserId, documentToDeny);

            return Ok();
        }

        [HttpPatch("/api/documents/sign/{documentId}")]
        public async Task<ActionResult> Sign(Guid documentId, [FromForm] SignAndParaphe doc)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            doc.ParapheImage = !doc.ParapheImage!.Contains("image/png") ? null : doc.ParapheImage;
            doc.SignImage = !doc.SignImage!.Contains("image/png") ? null : doc.SignImage;

            // await _userDocumentRepository.UpdateSignAndParaphe(documentId, currentUserId, doc.SignImage, doc.ParapheImage);

            await _documentsProcessesRepository.ValidateDocument(documentId, currentUserId, "");

            return Ok();
        }

        [HttpGet("/api/documents/{documentId}/former_document_steps")]
        public async Task<ActionResult> GetFormerDocumentSteps(Guid documentId)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var formerDocumentSteps = await _documentsProcessesRepository.GetFormerDocumentSteps(documentId, currentUserId);

            return Ok(formerDocumentSteps);
        }

        [HttpGet("/api/document_steps/{documentStepId}/users")]
        public async Task<ActionResult> GetUsersDocumentStep(Guid documentStepId)
        {
            var users = await _documentsProcessesRepository.GetUsersDocumentStep(documentStepId);

            return Ok(users);
        }

        [HttpPost("/api/documents/{documentId}/redirect")]
        public async Task<IActionResult> RedirectUser(Guid documentId, NewDocumentRedirection newDocumentRedirection)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _documentsProcessesRepository.Redirect(documentId, currentUserId, newDocumentRedirection);

            return Ok();
        }
    }
}
