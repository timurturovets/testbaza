using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Extensions;
using TestBaza.Repositories.Tests;

namespace TestBaza.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ITestsRepository _testsRepo;
        public AdminController(
            UserManager<User> userManager,
            ITestsRepository testsRepo)
        {
            _userManager = userManager;
            _testsRepo = testsRepo;
        }
        [Route("/admin-panel")]
        public async Task<IActionResult> AdminPanel()
        {
            if (!await CheckIfAdmin()) return Forbid();
            return View();
        }

        [HttpGet("/api/admin/delete-test")]
        public async Task<IActionResult> DeleteTest([FromQuery] int id)
        {
            if (!await CheckIfAdmin()) return Forbid();

            var test = await _testsRepo.GetTestAsync(id);
            if (test is null) return NotFound();

            await _testsRepo.RemoveTestAsync(test);
            return Ok();
        }

        [HttpGet("/api/admin/delete-user")]
        public async Task<IActionResult> DeleteUser([FromQuery] string id)
        {
            if (!await CheckIfAdmin()) return Forbid();

            var user = _userManager.Users.FirstOrDefault(u => u.Id == id);
            if (user is null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded ? Ok() : Conflict();
        }
        private async Task<bool> CheckIfAdmin()
        {
            var adminName = HttpContext
                .GetService<IConfiguration>()
                .GetValue<string>("ADMIN_USERNAME");

            var currentUser = await _userManager.GetUserAsync(User);
            return adminName == currentUser?.UserName;
        }
    }
}