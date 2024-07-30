using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class DocumentController : Controller
    {
        [HttpGet("new_document")]
        public IActionResult NewDocument()
        {
            return View();
        }

        [HttpGet("documents")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("documents/{Id}")]
        public IActionResult Details()
        {
            return View();
        }

        [HttpGet("authenticate_document")]
        public IActionResult AuthenticateDocument()
        {
            return View();
        }

        [HttpGet("documents_dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
