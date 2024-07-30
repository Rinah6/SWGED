using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class NotFoundController : Controller
    {
        [HttpGet("404")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
