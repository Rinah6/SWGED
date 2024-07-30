using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/numeric_library")]
        public IActionResult NumericLibrary()
        {
            return View();
        }
    }
}
