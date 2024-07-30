using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class SharedDocumentController : Controller
    {
        [HttpGet("documents/shared/{Id}")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
