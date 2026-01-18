using Microsoft.AspNetCore.Mvc;

namespace School_System.Controllers
{
    public class ParentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
