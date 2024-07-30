using API.Dto;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachementController : ControllerBase
    {
        private readonly AttachementRepository _attachementRepository;

        public AttachementController(AttachementRepository attachementRepository)
        {
            _attachementRepository = attachementRepository;
        }

        [HttpGet("/api/documents/{documentId}/attachements")]
        [AllowAnonymous]
        public async Task<ActionResult> GetAttachements(Guid documentId)
        {
            var liste = await _attachementRepository.FindByDocumentId(documentId);

            return Ok(liste);
        }

        [HttpGet("/api/download/attachements/{Id}")]
        [AllowAnonymous]
        public async Task<ActionResult> Download(Guid Id)
        {
            var document = await _attachementRepository.Get(Id);

            if (document == null)
            {
                return NotFound();
            }

            var fileStream = new FileStream(document.Url, FileMode.Open, FileAccess.Read);
            var contentType = "application/octet-stream";

            return File(fileStream, contentType, document.Filename);
        }

        [HttpPost("add/{documentId}")]
        public async Task<ActionResult<string>> AddAttachements(Guid documentId, [FromForm] AttachementDto doc)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            await _attachementRepository.AddAttachements(doc.Attachements, documentId, currentUserId);

            return "Finish";
        }

        [HttpDelete("delete/{Id}")]
        public async Task<ActionResult<string>> DeleteAttachement(Guid id)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            await _attachementRepository.Delete(id);

            return "Finish";
        }

        [HttpPut("rename/{Id}")]
        public async Task<ActionResult<string>> RenameAttachement(string id, [FromForm] AttachementDto doc)
        {

            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            await _attachementRepository.Rename(id, doc.Filename);

            return "Finish";
        }

        [HttpGet("render/{Id}")]
        public async Task<ActionResult> RenderAttachement(Guid id)
        {
            var img = new ConvertToImage();
            var attachement = await _attachementRepository.Get(id);
            string[] extension = attachement.Url.Split(".");
            string ext = extension[extension.Length - 1];

            if (ext.ToLower() == "pdf")
            {
                var stream = await img.ConvertTxtToPDF(attachement.Url);
                Console.WriteLine(attachement.Url);
                
                return File(stream, "application/pdf");

            }
            if (ext.ToLower() == "txt")
            {
                var stream = await img.ConvertTxtToPDF(attachement.Url);
                return File(stream, "text/plain");
            }
            //if (ext.ToLower().Contains("doc"))
            //{
            //    var stream = await img.ConvertTxtToPDF(attachement.Url);
            //    return File(stream, "application/msword", "file.doc");
            //}
            //if (ext.ToLower().Contains("xl") || ext.ToLower().Contains("csv"))
            //{
            //    var stream = await img.ConvertTxtToPDF(attachement.Url);
            //    return File(stream, "application/vnd.ms-excel", "file.xls");
            //}
            //if (ext.ToLower().Contains("ppt"))
            //{
            //    var stream = await img.ConvertTxtToPDF(attachement.Url);
            //    return File(stream, "application/vnd.ms-powerpoint", "file.ppt");
            //}

            if (ext.Contains("jpeg", StringComparison.CurrentCultureIgnoreCase) || ext.Contains("jpg", StringComparison.CurrentCultureIgnoreCase))
            {
                byte[] stream = System.IO.File.ReadAllBytes(attachement.Url);
                return Ok("data: image/jpeg ;base64, " + Convert.ToBase64String(stream.ToArray()));
            }
            if (ext.Contains("png", StringComparison.CurrentCultureIgnoreCase))
            {
                var stream = await img.ConvertTxtToPDF(attachement.Url);
                return Ok("data: image/png ;base64, " + Convert.ToBase64String(stream.ToArray()));
            }
            if (ext.Contains("tif", StringComparison.CurrentCultureIgnoreCase))
            {
                byte[] stream = System.IO.File.ReadAllBytes(attachement.Url);
                return Ok("data: image/tiff ;base64, " + Convert.ToBase64String(stream.ToArray()));
            }
            else
            {
                byte[] stream = System.IO.File.ReadAllBytes(attachement.Url);
                return Ok("data: image/webp ;base64, " + Convert.ToBase64String(stream.ToArray()));
            }
        }
    }
}
