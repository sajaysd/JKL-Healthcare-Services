using Microsoft.AspNetCore.Mvc;

namespace JKLHealthcareSystem.Controllers
{
    public class DashboardController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
