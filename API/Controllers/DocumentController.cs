using Microsoft.AspNetCore.Mvc;
using API.Dto;
using API.Model;
using API.Services;
using API.Data.Entities;
using API.Data;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("api/documents")]
    [ApiController]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentRepository _documentRepository;
        private readonly DocumentService _documentService;
        private readonly IConfiguration _config;
        private readonly PdfService _pdfService;
        private readonly MailService _emailService;
        private readonly UserRepository _userRepository;

        public DocumentController(
            DocumentRepository documentRepository,
            DocumentService documentService,
            PdfService pdfService,
            MailService emailService,
            IConfiguration config,
            UserRepository userRepository
        )
        {
            _documentRepository = documentRepository;
            _documentService = documentService;
            _emailService = emailService;
            _pdfService = pdfService;
            _config = config;
            _userRepository = userRepository;
        }

        [HttpGet("received_from_suppliers")]
        public async Task<ActionResult> GetDocumentsSendedBySuppliers()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var documents = await _documentRepository.GetDocumentsSendedBySuppliers(currentUserId);

            return Ok(documents);
        }

        [HttpGet("received")]
        public async Task<ActionResult> GetReceivedDocuments()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var documents = await _documentRepository.GetReceivedDocuments(currentUserId);

            return Ok(documents);
        }

        [HttpGet("sended")]
        public async Task<ActionResult> GetSendedDocuments()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var userInfo = await _userRepository.GetUserDetails(currentUserId);

            if (userInfo == null)
            {
                return StatusCode(403);
            }

            var documents = await _documentRepository.GetSendedDocuments(currentUserId, userInfo.Username);

            return Ok(documents);
        }

        [HttpGet("")]
        public async Task<ActionResult> GetAllDocuments(DocumentStatus status)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var documents = await _documentRepository.GetDocuments(currentUserId, status);

            return Ok(documents);
        }

        [HttpGet("common_documents")]
        public async Task<ActionResult> GetCommonDocuments()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var documents = await _documentRepository.GetCommonDocuments(currentUserId);

            return Ok(documents);
        }

        [HttpGet("total_number_by_status")]
        public async Task<ActionResult> GetNumberOfDocumentsByStatus()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var currentUserRole = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "role")!.Value;

            var numberOfDocumentByStatus = await _documentRepository.GetNumberOfDocumentsByStatus(currentUserId, Enum.Parse<UserRole>(currentUserRole));

            return Ok(numberOfDocumentByStatus);
        }

        [HttpGet("/api/documents/suppliers/{supplierId}")]
        public async Task<ActionResult> GetDocumentsBySupplierId(Guid supplierId)
        {
            var documents = await _documentRepository.GetDocumentsBySupplierId(supplierId);

            return Ok(documents);
        }

        [HttpGet("{documentId}")]
        public async Task<ActionResult> GetDocumentById(Guid documentId)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var wasSendedByASupplier = await _documentRepository.WasSendedByASupplier(documentId);

            if (wasSendedByASupplier)
            {
                var supplierDocumentDetails = await _documentRepository.GetSuppliersDocumentDetails(documentId, currentUserId);

                return Ok(supplierDocumentDetails);
            }

            var document = await _documentRepository.GetWithUser(documentId, currentUserId);

            return Ok(document);
        }

        [HttpGet("/api/documents/{documentId}/details")]
        [AllowAnonymous]

        public async Task<ActionResult> GetDocumentDetails(Guid documentId)
        {
            var document = await _documentRepository.GetDocumentDetails(documentId);

            return Ok(document);
        }

        [HttpGet("/api/pdf/{documentId}")]
        public async Task<ActionResult> GetPdf(Guid documentId)
        {
            var document = await _documentRepository.Get(documentId);

            if (document == null)
            {
                return NotFound();
            }

            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var pdf = await _pdfService.GeneratePDF(documentId, document, currentUserId);

            if (pdf == null)
            {
                return NotFound();
            }

            // return File(pdf, "application/pdf", document.OriginalFilename);
            return File(pdf, "application/octet-stream", document.OriginalFilename);
        }

        [HttpGet("/api/documents/archived/{documentId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetArchivedDocument(Guid documentId)
        {
            var document = await _documentRepository.Get(documentId);

            if (document == null)
            {
                return NotFound();
            }

            var pdf = await _pdfService.GeneratePDF(documentId, document);

            if (pdf == null)
            {
                return NotFound();
            }

            // return File(pdf, "application/pdf", document.OriginalFilename);
            return File(pdf, "application/octet-stream", document.OriginalFilename);
        }

        [HttpGet("name/{documentId}")]
        [AllowAnonymous]

        public async Task<ActionResult> GetDocumentNameById(Guid documentId)
        {
            var document = await _documentRepository.Get(documentId);

            if (document == null)
            {
                return NotFound();
            }

            return Ok(document.OriginalFilename);
        }

        [HttpPost("share/{documentId}")]
        [AllowAnonymous]
        public async Task<ActionResult> Share(Guid documentId, string email)
        {
            var document = await _documentRepository.Get(documentId);

            if (document == null)
            {
                return NotFound();
            }

            Dictionary<string, string> parameterList = new Dictionary<string, string>
            {
                { "NOM", document.Title },
                { "URL",  _config["Url"]+"documents/shared/" + documentId}
            };

            var receiver = email.Split(";").ToList();

            receiver = receiver.Where(u => !string.IsNullOrEmpty(u)).ToList();

            await _emailService.SendAppropriateMail(MailType.ShareLink, parameterList, receiver);

            return Ok();
        }

        [HttpPatch("physical_location")]
        public async Task<ActionResult> UpdateDocumentPhysicalLocation(DocumentPhysicalLocationToUpdate documentPhysicalLocationToUpdate)
        {
            await _documentRepository.UpdateDocumentPhysicalLocation(documentPhysicalLocationToUpdate.Id, documentPhysicalLocationToUpdate.PhysicalLocation);

            return Ok();
        }

        [HttpPatch("changefile/{documentId}")]
        public async Task<ActionResult> ChangeFile(Guid documentId, [FromForm] AttachementDto file)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var document = await _documentRepository.GetWithAttachement(documentId);

            await _documentService.UpdateAttachement(document, file.Attachements.FirstOrDefault(), currentUserId);

            return Ok();
        }

        [HttpPatch("changefilename/{documentId}")]
        public async Task<ActionResult> ChangeFileName(Guid documentId, [FromForm] AttachementDto file)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _documentRepository.ChangeFileName(documentId, file.Filename);

            return Ok();
        }
    }
}
