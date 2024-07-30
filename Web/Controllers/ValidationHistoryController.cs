using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class ValidationHistoryController : Controller
    {
        [HttpGet("documents/{Id}/validation_history")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
