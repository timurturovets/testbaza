using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

using TestBaza.Models;

namespace TestBaza.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult ForbidPage() => View();
        public IActionResult NotFoundPage() => View();
    }
}