using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet("projects")]
        public IActionResult ProjectsManagement()
        {
            return View();
        }

        [HttpGet("soas")]
        public IActionResult SoasManagement()
        {
            return View();
        }

        [HttpGet("users")]
        public IActionResult UsersManagement()
        {
            return View();
        }

        [HttpGet("dashboard/suppliers_documents_receivers")]
        public IActionResult SuppliersDocumentsReceivers()
        {
            return View();
        }

        [HttpGet("dashboard/suppliers")]
        public IActionResult Suppliers()
        {
            return View();
        }

        [HttpGet("dynamic_fields")]
        public IActionResult DynamicFieldsManagement()
        {
            return View();
        }

        [HttpGet("tom_pro_connections")]
        public IActionResult TomProConnectionsManagement()
        {
            return View();
        }

        [HttpGet("users_connections")]
        public IActionResult UsersConnectionsHistory()
        {
            return View();
        }
    }
}
