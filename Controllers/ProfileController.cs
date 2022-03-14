using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Models;
using TestBaza.Extensions;
using TestBaza.Repositories;

namespace TestBaza.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ITestsRepository _testsRepo;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ProfileController> _logger;
        public ProfileController(
            ITestsRepository testsRepo,
            UserManager<User> userManager,
            ILogger<ProfileController> logger
            )
        {
            _testsRepo = testsRepo;
            _userManager = userManager;
            _logger = logger;
        }

        [Route("/profile")]
        public async Task<IActionResult> Get()
        {
            User user = await _userManager.GetUserAsync(User);
            return View(viewName: "main");
        }
        [HttpGet("/profile/user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            User user = await _userManager.GetUserAsync(User);
            return Ok(new { result = user.ToJsonModel() });
        }
        [HttpGet("/profile/tests-user{id}")]
        public async Task<IActionResult> GetUserTests([FromRoute] string id)
        {
            User user = await _userManager.GetUserAsync(User);
            User creator = await _userManager.FindByIdAsync(id);

            if (creator is null) return NotFound();

            if (!user.Equals(creator)) return Forbid();

            if (!user.Tests.Any()) return NoContent();

            IEnumerable<TestSummary> summaries = creator.Tests.Select(t => t.ToSummary());
            return Ok(new { result = summaries });
        }

        [HttpPost("/profile/update-user")]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserRequestModel model)
        {
            _logger.LogError($"New update user request, name: {model.UserName}, " +
                $"email: {model.Email}, password: {model.Password}");
            if (ModelState.IsValid)
            {
                User user = await _userManager.GetUserAsync(User);
                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                    return BadRequest(new { errors = new[] { "Вы ввели неверный пароль" } });

                user.UserName = model.UserName;
                if (user.Email != model.Email) user.EmailConfirmed = false;
                user.Email = model.Email;
                await _userManager.UpdateAsync(user);

                return Ok();
            }
            else
            {
                string[] errors = ModelState.Select(entry => {
                    var query = entry.Value?.Errors.Select(e => e.ErrorMessage);
                    if (query is null || !query.Any()) return string.Empty;
                    else return query.Aggregate((x, y) => x + ", " + y);
                    }).Select(value=>value+". ").ToArray();
                return BadRequest(new { errors });
            }
        }

        [HttpPost("/profile/change-password")]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordRequestModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.GetUserAsync(User);
                IdentityResult result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded) return Ok();
                else return BadRequest(new { error = new[] { "Вы ввели неверный пароль" } });
            }
            else return BadRequest(new { error = ModelState.ToStringEnumerable() });
        }
    }
}