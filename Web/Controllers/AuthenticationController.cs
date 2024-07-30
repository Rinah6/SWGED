using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
