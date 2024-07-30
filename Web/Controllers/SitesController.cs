using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class SitesController : Controller
    {


        [HttpGet("gestion_sites")]
        public IActionResult Index()
        {
            return View();
        }


    }
}
