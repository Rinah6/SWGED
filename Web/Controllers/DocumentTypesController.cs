using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class DocumentTypesController : Controller
    {
        [HttpGet("document_types")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
