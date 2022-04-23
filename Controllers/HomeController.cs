using Microsoft.AspNetCore.Mvc;

namespace TestBaza.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult ForbidPage() => View();
        public IActionResult NotFoundPage() => View();
    }
}