using Microsoft.AspNetCore.Mvc;
using API.Data.Entities;
using API.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using API.Services;
using API.Data.Entities.Dto;
using Newtonsoft.Json;
using API.Data;

namespace API.Controllers
{
    [Route("api/suppliers")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "jfien434YUGfbjjr94")]
    public class SuppliersController : ControllerBase
    {
        private readonly ProjectRepository _projectRepository;
        private readonly SupplierRepository _supplierRepository;
        private readonly DocumentService _documentService;
        private readonly DynamicFieldRepository _dynamicFieldRepository;
        private readonly ProjectDocumentsReceiverRepository _projectDocumentsReceiverRepository;
        private readonly DocumentRepository _documentRepository;
        private readonly MailService _mailService;

        public SuppliersController(
            ProjectRepository projectRepository,
            SupplierRepository supplierRepository,
            DocumentService documentService,
            DynamicFieldRepository dynamicFieldRepository,
            ProjectDocumentsReceiverRepository projectDocumentsReceiverRepository,
            DocumentRepository documentRepository,
            MailService MailService
        )
        {
            _projectRepository = projectRepository;
            _supplierRepository = supplierRepository;
            _documentService = documentService;
            _dynamicFieldRepository = dynamicFieldRepository;
            _projectDocumentsReceiverRepository = projectDocumentsReceiverRepository;
            _documentRepository = documentRepository;
            _mailService = MailService;
        }

        [HttpPost("auth")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(Supplier supplier)
        {
            // var supplierId = await _supplierRepository.GetSupplierId(supplier);

            Guid? supplierId;

            if (supplier.NIF == null || supplier.STAT == null)
            {
                supplierId = await _supplierRepository.GetSupplieIdWithoutNIFAndSTAT(supplier.Name, supplier.ProjectId, supplier.CIN);
            }
            else
            {
                supplierId = await _supplierRepository.GetSupplierId(supplier);
            }

            if (supplierId == null)
            {
                return StatusCode(403);
            }

            var projet = await _projectRepository.Get(supplier.ProjectId);

            if (projet == null)
            {
                return StatusCode(403);
            }

            var claimsIdentity = new ClaimsIdentity(new[] {
                new Claim("Id", supplierId.ToString()!),
                new Claim("NIF", supplier.NIF ?? ""),
                new Claim("STAT", supplier.STAT ?? ""),
                new Claim("MAIL", supplier.MAIL ?? ""),
                new Claim("CONTACT", supplier.CONTACT ?? ""),
                new Claim("Name", supplier.Name),
                new Claim("ProjectId", supplier.ProjectId.ToString()),
                new Claim("Project", projet.Name)
            }, "jfien434YUGfbjjr94");

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await Request.HttpContext.SignInAsync("jfien434YUGfbjjr94", claimsPrincipal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddSeconds(12 * 60 * 60)
            });

            return Ok();
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register(Supplier supplier)
        {
            Guid? supplierId;

            if (supplier.NIF == null || supplier.STAT == null)
            {
                supplierId = await _supplierRepository.GetSupplieIdWithoutNIFAndSTAT(supplier.Name, supplier.ProjectId, supplier.CIN);
            }
            else
            {
                supplierId = await _supplierRepository.CheckSupplier(supplier);
            }

            if (supplierId != null)
            {
                return StatusCode(403);
            }

            await _supplierRepository.RegisterSupplier(supplier);

            return Ok();
        }

        [HttpGet("project")]
        [Authorize(AuthenticationSchemes = "10mOyIm3S1WMbwaCE7")]
        public async Task<IActionResult> GetSuppliersByProjectId()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            var suppliers = await _supplierRepository.GetSuppliersByProjectId(project.Id);

            return Ok(suppliers);
        }

        [HttpDelete("{Id}/project")]
        [Authorize(AuthenticationSchemes = "10mOyIm3S1WMbwaCE7")]
        public async Task<IActionResult> DeleteSupplier(string Id)
        {
            await _supplierRepository.DeleteSupplier(Id);

            return Ok();
        }

        [HttpGet("check_projects")]
        [AllowAnonymous]
        public async Task<ActionResult> CheckSuppliersProject(string Id)
        {
            var projectId = Guid.TryParse(Id, out _);

            if (!projectId)
            {
                return BadRequest();
            }

            var project = await _projectRepository.Get(new Guid(Id));

            if (project == null)
            {
                return NotFound();
            }

            if (!project.HasAccessToSuppliersHandling)
            {
                return StatusCode(403);
            }

            return Ok();
        }

        [HttpGet("credentials")]
        public async Task<ActionResult> CheckSupplierCredentials()
        {
            var currentUserProjectId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "ProjectId")!.Value;
            var currentProject = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Project")!.Value;

            var project = await _projectRepository.Get(new Guid(currentUserProjectId));

            if (project == null)
            {
                return NotFound();
            }

            return Ok(new SupplierCredentials
            {
                ProjectId = currentUserProjectId,
                Project = currentProject,
                HasAccessToGlobalDynamicFieldsHandling = project.HasAccessToGlobalDynamicFieldsHandling
            });
        }

        [HttpDelete("/api/suppliers/logout")]
        public async Task<IActionResult> Logout()
        {
            await Request.HttpContext.SignOutAsync("jfien434YUGfbjjr94");

            return Ok();
        }

        [HttpPost("documents")]
        public async Task<ActionResult> ArchiveDocument([FromForm] SupplierDocumentToAdd supplierDocumentToAdd)
        {
            var currentSupplierId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var currentSupplierProjectId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "ProjectId")!.Value);
            var currentSupplierName = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Name")!.Value;
            var currentSupplierNIF = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "NIF")!.Value;
            var currentSupplierSTAT = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "STAT")!.Value;

            var document = await _documentService.CreateDocument(supplierDocumentToAdd.PdfDocument, supplierDocumentToAdd.Attachements, supplierDocumentToAdd.Title, supplierDocumentToAdd.Object, supplierDocumentToAdd.Message, false, currentSupplierId, DocumentStatus.None, supplierDocumentToAdd.Site);

            if (document == null)
            {
                return StatusCode(403);
            }

            var globalDynamicFields = JsonConvert.DeserializeObject<List<GlobalDynamicFieldDto>>(supplierDocumentToAdd.GlobalDynamicFields)!;

            for (int i = 0; i < globalDynamicFields.Count; i += 1)
            {
                await _dynamicFieldRepository.AddDocumentDynamicField(document.Id, globalDynamicFields[i].Id, globalDynamicFields[i].Value);
            }

            var projectDocumentsReceivers = await _projectDocumentsReceiverRepository.GetProjectDocumentsReceivers(currentSupplierProjectId);

            await _projectDocumentsReceiverRepository.PostToDocumentsReceptions(document.Id, projectDocumentsReceivers.Select(projectDocumentsReceiver => projectDocumentsReceiver.Id).ToList());

            await _supplierRepository.RegisterSupplierEmail(document.SenderId.ToString(), supplierDocumentToAdd.Email);

            var documentDynamicFields = await _dynamicFieldRepository.GetAllDynamicFieldsByDocumentId(document.Id);

            var dynamicFields = "";

            for (int i = 0; i < documentDynamicFields.Count; i += 1)
            {
                if (i > 0)
                {
                    dynamicFields += "<br />";
                }

                dynamicFields += $"<b><u>{documentDynamicFields[i].Label}</u></b> : {documentDynamicFields[i].Value}";
            }

            var mailBody = @$"
                <div>
                    Madame, Monsieur, 
                    <br /><br />
                    Nous vous confirmons par le présent mail que le document que vous avez créé a bien été soumis. 

                    <br /><br />
                    <b><u>Titre du document</u></b>: {supplierDocumentToAdd.Title} <br />
                    <b><u>Objet</u></b>: {supplierDocumentToAdd.Object} <br />
                    <b><u>Message</u></b>: {supplierDocumentToAdd.Message} <br /><br />

                    <b><u>NIF</u></b>: {currentSupplierNIF} <br />
                    <b><u>STAT</u></b>: {currentSupplierSTAT} <br /><br />

                    {dynamicFields} <br /><br />
                    Cordialement.
                </div>
            ";

            await _mailService.SendEmail("SoftGED - Création d'un document", mailBody, new List<string> { supplierDocumentToAdd.Email! });

            mailBody = @$"
                <div>
                    Madame, Monsieur, 
                    <br /><br />
                    Nous vous confirmons par le présent mail qu'un document venant de ""{currentSupplierName}"" a bien été soumis. 

                    <br /><br />
                    <b><u>Titre du document</u></b>: {supplierDocumentToAdd.Title} <br />
                    <b><u>Objet</u></b>: {supplierDocumentToAdd.Object} <br />
                    <b><u>Message</u></b>: {supplierDocumentToAdd.Message} <br /><br />

                    <b><u>NIF</u></b>: {currentSupplierNIF} <br />
                    <b><u>STAT</u></b>: {currentSupplierSTAT} <br /><br />

                    {dynamicFields} <br /><br />
                    Cordialement.
                </div>
            ";

            await _mailService.SendEmail("SoftGED - Création d'un document", mailBody, projectDocumentsReceivers.Select(projectDocumentsReceiver => projectDocumentsReceiver.Email).ToList());

            return Ok(document.Id);
        }

        [HttpPost("documents/{documentId}/acknowledge")]
        [Authorize(AuthenticationSchemes = "10mOyIm3S1WMbwaCE7")]
        public async Task<ActionResult> AcknowledgeDocument(Guid documentId)
        {
            var document = await _documentRepository.Get(documentId);

            if (document == null)
            {
                return NotFound();
            }

            var supplier = await _supplierRepository.GetSupplierByDocumentId(documentId);

            if (supplier == null)
            {
                return NotFound();
            }

            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _supplierRepository.SetWasAcknowledged(documentId, currentUserId);

            var documentDynamicFields = await _dynamicFieldRepository.GetAllDynamicFieldsByDocumentId(document.Id);

            var dynamicFields = "";

            for (int i = 0; i < documentDynamicFields.Count; i += 1)
            {
                if (i > 0)
                {
                    dynamicFields += "<br />";
                }

                dynamicFields += $"<b><u>{documentDynamicFields[i].Label}</u></b> : {documentDynamicFields[i].Value}";
            }

            var mailBody = @$"
                <div>
                    Madame, Monsieur, 
                    <br /><br>Nous vous accusons réception par le présent mail que le document que vous avez envoyé a bien été reçu.

                    <br /><br />
                    <b><u>Titre du document</u></b>: {document.Title} <br />
                    <b><u>Objet</u></b>: {document.Object} <br />
                    <b><u>Message</u></b>: {document.Message} <br /><br />

                    <b><u>NIF</u></b>: {supplier.NIF ?? ""} <br />
                    <b><u>STAT</u></b>: {supplier.STAT ?? ""} <br /><br />
                    <b><u>Nom</u></b>: {supplier.Name ?? ""} <br /><br />

                    {dynamicFields} <br /><br />
                    Cordialement.
                </div>
            ";

            await _mailService.SendEmail("Soumission d'un document dans SoftGED", mailBody, new List<string> { supplier.Email! });

            return Ok();
        }
    }
}
