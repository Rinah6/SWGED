using API.Data;
using API.Data.Entities;
using API.Data.Entities.Dto;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [ApiController]
    public class SignatureController : ControllerBase
    {
        private readonly SignatureRepository _signatureRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly DocumentService _documentService;
        private readonly DynamicFieldRepository _dynamicFieldRepository;
        private readonly DocumentsProcessesRepository _documentsProcessesRepository;
        private readonly UserRepository _userRepository;

        public SignatureController(
            SignatureRepository signatureRepository,
            DocumentsProcessesRepository documentsProcessesRepository,
            ProjectRepository projectRepository,
            DocumentService documentService,
            DynamicFieldRepository dynamicFieldRepository,
            UserRepository userRepository
        )
        {
            _signatureRepository = signatureRepository;
            _documentsProcessesRepository = documentsProcessesRepository;
            _projectRepository = projectRepository;
            _documentService = documentService;
            _dynamicFieldRepository = dynamicFieldRepository;
            _userRepository = userRepository;
        }

        [HttpPost("/api/signatures/extract")]
        public async Task<IActionResult> ExtractSignatureFromImage([FromForm] SignatureImageToExtract signatureImageToExtract)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var signatureId = await _signatureRepository.ExtractSignatureFromImage(signatureImageToExtract.File, currentUserId);

            return Ok(signatureId);
        }

        [HttpGet("/api/signatures/{signatureId}")]
        public async Task<IActionResult> GetSignature(Guid signatureId)
        {
            var memoryStream = await _signatureRepository.GetSignature(signatureId);

            if (memoryStream == null)
            {
                return NotFound();
            }

            return File(memoryStream, "image/png");
        }

        [HttpPost("/api/signatures/send_token")]
        public async Task<IActionResult> SendToken(Guid signatureId)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var currentUser = await _userRepository.Get(currentUserId);

            if (currentUser == null)
            {
                return StatusCode(403);
            }

            await _signatureRepository.UpdateVerificationTokens(signatureId, currentUserId, currentUser.Email);

            return Ok();
        }

        [HttpPost("/api/signatures/check_token_authenticity")]
        public async Task<IActionResult> CheckToken(TokenToVerify tokenToVerify)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            if (!await _signatureRepository.VerifyToken(currentUserId, tokenToVerify.Token))
            {
                return StatusCode(403);
            }

            return Ok();
        }

        [HttpPost("/api/sign_document")]
        public async Task<ActionResult> SignDocument([FromForm] DocumentToSign documentToSign)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            var document = await _documentService.CreateDocument(documentToSign.DocumentFile, documentToSign.Attachements, documentToSign.Title, documentToSign.Object, documentToSign.Message, Convert.ToBoolean(documentToSign.RSF), currentUserId, DocumentStatus.Archived, documentToSign.Site);

            if (document == null)
            {
                return NotFound();
            }

            var globalDynamicFields = JsonConvert.DeserializeObject<List<GlobalDynamicFieldDto>>(documentToSign.GlobalDynamicFields)!;

            for (int i = 0; i < globalDynamicFields.Count; i += 1)
            {
                await _dynamicFieldRepository.AddDocumentDynamicField(document.Id, globalDynamicFields[i].Id, globalDynamicFields[i].Value);
            }

            await _documentsProcessesRepository.UpdateDocumentStatus(document.Id, DocumentStatus.Archived);

            var field = JsonConvert.DeserializeObject<Field>(documentToSign.FieldDetails)!;

            await _signatureRepository.SignDocument(document.Url, new DigitalSignaturePayload
            {
                DocumentId = document.Id,
                SignatureId = Guid.Parse(documentToSign.SignatureId),
                Token = documentToSign.Token,
                PageIndex = (uint)field.FirstPage - 1,
                X = (float)field.X,
                Y = (float)field.Y,
            });

            return Ok(document.Id);
        }

        [HttpPost("/api/verify_signed_document")]
        public async Task<IActionResult> VerifySignedDocument(SignedDocumentToVerify signedDocumentToVerify)
        {
            var res = await _signatureRepository.VerifySignedDocument(signedDocumentToVerify.DocumentId, signedDocumentToVerify.DigitalSignatureId);

            if (!res)
            {
                return StatusCode(403);
            }

            return Ok();
        }
    }
}
