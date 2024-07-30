using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/validationsHistoryRepository")]
    [ApiController]
    [Authorize]
    public class ValidationsHistoryController : ControllerBase
    {
        private readonly ValidationsHistoryRepository _validationsHistoryRepository;

        public ValidationsHistoryController(ValidationsHistoryRepository validationsHistoryRepository)
        {
            _validationsHistoryRepository = validationsHistoryRepository;
        }

        // [HttpGet("{Id}")]
        // public async Task<ActionResult<FlowHistoriqueDto>?> GetDocumentById(string Id)
        // {
        //     var document = await _validationsHistoryRepository.GetByDocument(Id);

        //     return Ok(_mapper.Map<List<FlowHistoriqueDto>?>(document));
        // }

        [HttpGet("/api/late_documents")]
        public async Task<IActionResult> GetLateDocuments()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var documents = await _validationsHistoryRepository.GetLateDocuments(currentUserId);

            return Ok(documents);
        }

        [HttpGet("/api/non_late_documents")]
        public async Task<IActionResult> GetNonLateDocuments()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var documents = await _validationsHistoryRepository.GetNonLateDocuments(currentUserId);

            return Ok(documents);
        }

        [HttpGet("/api/documents/{documentId}/validation_history")]
        public async Task<IActionResult> GetValidationHistoryDetails(Guid documentId)
        {
            var validationHistoryDetails = await _validationsHistoryRepository.GetValidationHistoryDetails(documentId);

            return Ok(validationHistoryDetails);
        }
    }
}
