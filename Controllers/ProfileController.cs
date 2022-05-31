using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Extensions;
using TestBaza.Models.DTOs;
using TestBaza.Repositories.Tests;
using TestBaza.Repositories.CheckInfos;
using TestBaza.Repositories.PassingInfos;

namespace TestBaza.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ITestsRepository _testsRepo;
    private readonly IPassingInfoRepository _passingInfoRepo;
    private readonly IChecksInfoRepository _checksRepo;
    private readonly UserManager<User> _userManager;
    public ProfileController(
        ITestsRepository testsRepo,
        IPassingInfoRepository passingInfoRepo,
        IChecksInfoRepository checksRepo,
        UserManager<User> userManager
        )
    {
        _testsRepo = testsRepo;
        _passingInfoRepo = passingInfoRepo;
        _checksRepo = checksRepo;

        _userManager = userManager;
    }

    [Route("/profile")]
    public IActionResult Get() => View("Main");

    [Route("/profile/user-tests")]
    public IActionResult UserTests() => View();

    [Route("/profile/check-tests{id:int}")]
    public async Task<IActionResult> CheckTests([FromRoute] int id)
    {
        var test = await _testsRepo.GetTestAsync(id);
        if (test is null) return NotFound();

        ViewData["TestId"] = id;
        return View();
    }

    [Route("/profile/test-stats{id:int}")]
    public async Task<IActionResult> TestStats([FromRoute] int id)
    {
        var test = await _testsRepo.GetTestAsync(id);
        if (test is null) return NotFound();

        var creator = await _userManager.GetUserAsync(User);
        if (!test.Creator!.Equals(creator)) return Forbid();

        ViewData["TestId"] = id;
        return View();
    }

    [HttpGet("/api/profile/checks-info")]
    public async Task<IActionResult> GetChecks([FromQuery] int testId)
    {
        var test = await _testsRepo.GetTestAsync(testId);
        if (test is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (!test.Creator!.Equals(user)) return Forbid();

        var infos = _checksRepo.GetUserCheckInfos(user).ToList();
        var summaries = infos
            .Where(i => i.Attempt?.PassingInfo?.Test?.Equals(test) ?? false)
            .Select(i => i.ToSummary());

        return Ok(new {result = summaries});
    }

    [HttpGet("/api/profile/get-stat")]
    public async Task<IActionResult> GetUserStat([FromQuery] int testId, [FromQuery] int attemptId)
    {
        var test = await _testsRepo.GetTestAsync(testId);
        if (test is null) return NotFound();

        var creator = await _userManager.GetUserAsync(User);
        if (!test.Creator!.Equals(creator)) return Forbid();

        var attempt = test.PassingInfos
            .SingleOrDefault(i => i.Attempts
                .Any(a => a.AttemptId == attemptId))
            ?.Attempts.SingleOrDefault(a => a.AttemptId == attemptId);

        if (attempt is null) return NotFound();

        var model = attempt.ToDetailedTest();
        return Ok(new {result = model});
    }

    [HttpGet("/api/profile/get-stats")]
    public async Task<IActionResult> GetUserStats([FromQuery] int id)
    {
        var test = await _testsRepo.GetTestAsync(id);
        if (test is null) return NotFound();

        var creator = await _userManager.GetUserAsync(User);
        if (!test.Creator!.Equals(creator)) return Forbid();

        var infos = test.PassingInfos.ToList();
        if (!infos.Any()) return NoContent();

        var summaries = infos
            .Select(i => i.Attempts
                .Where(a => a.IsEnded)
                .OrderBy(a => a.TimeEnded)
                .LastOrDefault())
            .Select(a => a!.ToStatSummary()).ToList();

        return !summaries.Any()
            ? NoContent()
            : Ok(new {result = summaries});
    }

    [HttpGet("/api/profile/user-info")]
    public async Task<IActionResult> GetUserInfo()
    {
        var user = await _userManager.GetUserAsync(User);
        return Ok(new{result=user.ToJsonModel()});
    }

    [HttpPost("/api/profile/update-user")]
    public async Task<IActionResult> UpdateUser([FromForm] UpdateUserDto model)
    {
        if (!ModelState.IsValid) 
            return BadRequest(new {result = ModelState.ToStringEnumerable()});
        
        var user = await _userManager.GetUserAsync(User);
        if (!await _userManager.CheckPasswordAsync(user, model.Password))
            return BadRequest(new {result = new[] { "Вы ввели неверный пароль" }} );

        user.UserName = model.UserName;
        if (user.Email != model.Email) user.EmailConfirmed = false;
        user.Email = model.Email;
        await _userManager.UpdateAsync(user);

        return Ok();

    }

    [HttpPost("/api/profile/change-password")]
    public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordDto model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (result.Succeeded) return Ok();
            return BadRequest(new {result = new[] {"Вы ввели неверный пароль"}});
        }

        return BadRequest(new {result = ModelState.ToStringEnumerable()});
    }

    [HttpGet("/api/profile/user-tests")]
    public async Task<IActionResult> GetUserTests()
    {
        var creator = await _userManager.GetUserAsync(User);

        var tests = _testsRepo.GetUserTests(creator).Select(t => t.ToSummary()).ToList();

        return !tests.Any() 
            ? NoContent()
            : Ok(new {result=tests} );
    }

    [HttpGet("/api/profile/passed-tests-info")]
    public async Task<IActionResult> GetPassedTests()
    {
        var user = await _userManager.GetUserAsync(User);
        var infos = _passingInfoRepo.GetUserInfos(user).ToList();

        if (!infos.Any()) return NoContent();

        var summaries = infos.Select(i => {
            var model = i.ToPassedTestSummary();
            model.UserRate = i.Test!.Rates
                .SingleOrDefault(r => r.User!.Equals(user))?.Value ?? -1;
            return model;
        });

        return Ok(new{result=summaries});
    }

    [HttpGet("/api/profile/detailed-test")]
    public async Task<IActionResult> GetDetailedTest([FromQuery] int testId)
    {
        var test = await _testsRepo.GetTestAsync(testId);
        if (test is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);

        var info = await _passingInfoRepo.GetInfoAsync(user, test);
        if (info is null) return Conflict();

        var lastAttempt = info.Attempts.OrderBy(a => a.TimeEnded).LastOrDefault();
        if (lastAttempt is null) return Conflict();

        var model = lastAttempt.ToDetailedTest();

        return Ok(new{result = model});
    }

    [HttpPost("/api/profile/check-test")]
    public async Task<IActionResult> CheckTest([FromForm] CheckTestDto model)
    {
        var test = await _testsRepo.GetTestAsync(model.TestId);
        if (test is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (!user.Equals(test.Creator)) return Forbid();

        var attempt = test.PassingInfos
            .FirstOrDefault(i => i.Attempts
                            .Any(a => a.AttemptId == model.AttemptId))
            ?.Attempts.Single(a => a.AttemptId == model.AttemptId);

        if (attempt is null) return NotFound();
        if (!attempt.IsEnded) return Conflict();

        foreach (var number in model.CorrectAnswers)
            attempt.UserAnswers.First(a => a.QuestionNumber == number).IsCorrect = true;
        
        foreach (var number in model.IncorrectAnswers)
            attempt.UserAnswers.First(a => a.QuestionNumber == number).IsCorrect = false;

        attempt.CheckInfo!.IsChecked = true;
        await _testsRepo.UpdateTestAsync(test);

        return Ok();
    }
}