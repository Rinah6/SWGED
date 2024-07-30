using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class SuppliersController : Controller
    {
        [HttpGet("suppliers/{Id}")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("suppliers/new_document")]
        public IActionResult NewDocument()
        {
            return View();
        }
    }
}
