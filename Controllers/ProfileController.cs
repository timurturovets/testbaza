using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Models;
using TestBaza.Repositories;

namespace TestBaza.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ITestsRepository _testsRepo;
        private readonly UserManager<User> _userManager;

        public ProfileController(
            ITestsRepository testsRepo,
            UserManager<User> userManager
            )
        {
            _testsRepo = testsRepo;
            _userManager = userManager;
        }

        [HttpGet("/profile")]
        public async Task<IActionResult> Get()
        {
            User user = await _userManager.GetUserAsync(User);
            ViewData["UserId"] = user.Id;
            return View(viewName: "main");
        }

        [HttpGet("/profile/tests-user{id}")]
        public async Task<IActionResult> GetUserTests([FromRoute] string id)
        {
            User user = await _userManager.GetUserAsync(User);
            User creator = await _userManager.FindByIdAsync(id);

            if (!user.Equals(creator)) return Forbid();

            if (!user.Tests.Any()) return NoContent();

            IEnumerable<TestSummary> summaries = creator.Tests.Select(t => t.ToSummary());
            return Ok(new { result = summaries });
        }
    }
}