using Microsoft.AspNetCore.Mvc;

namespace Chotiskazal.RestApp.Controllers
{
    public class Home : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}