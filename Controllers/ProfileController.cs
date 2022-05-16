using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Factories;
using TestBaza.Extensions;
using TestBaza.Repositories;

namespace TestBaza.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ITestsRepository _testsRepo;
        private readonly IPassingInfoRepository _passingInfoRepo;
        private readonly IChecksInfoRepository _checksRepo;
        private readonly IResponseFactory _responseFactory;
        private readonly UserManager<User> _userManager;
        public ProfileController(
            ITestsRepository testsRepo,
            IPassingInfoRepository passingInfoRepo,
            IChecksInfoRepository checksRepo,

            IResponseFactory responseFactory,
            UserManager<User> userManager
            )
        {
            _testsRepo = testsRepo;
            _passingInfoRepo = passingInfoRepo;
            _checksRepo = checksRepo;

            _responseFactory = responseFactory;
            _userManager = userManager;
        }

        [Route("/profile")]
        public IActionResult Get() => _responseFactory.View(this, "main");

        [Route("/profile/user-tests")]
        public IActionResult UserTests() => _responseFactory.View(this);

        [Route("/profile/check-tests{id}")]
        public async Task<IActionResult> CheckTests([FromRoute] int id)
        {
            var test = await _testsRepo.GetTestAsync(id);
            if (test is null) return _responseFactory.NotFound(this);

            ViewData["TestId"] = id;
            return _responseFactory.View(this);
        }

        [Route("/profile/test-stats{id}")]
        public async Task<IActionResult> TestStats([FromRoute] int id)
        {
            var test = await _testsRepo.GetTestAsync(id);
            if (test is null) return _responseFactory.NotFound(this);

            var creator = await _userManager.GetUserAsync(User);
            if (!test.Creator!.Equals(creator)) return _responseFactory.Forbid(this);

            ViewData["TestId"] = id;
            return _responseFactory.View(this);
        }

        [HttpGet("/api/profile/checks-info")]
        public async Task<IActionResult> GetChecks([FromQuery] int testId)
        {
            var test = await _testsRepo.GetTestAsync(testId);
            if (test is null) return _responseFactory.NotFound(this);

            var user = await _userManager.GetUserAsync(User);
            if (!test!.Creator!.Equals(user)) return _responseFactory.Forbid(this);

            var infos = _checksRepo.GetUserCheckInfos(user).ToList();
            var summaries = infos
                .Where(i => i.Attempt?.PassingInfo?.Test?.Equals(test) ?? false)
                .Select(i => i.ToSummary());

            return _responseFactory.Ok(this, summaries);
        }

        [HttpGet("/api/profile/get-stat")]
        public async Task<IActionResult> GetUserStat([FromQuery] int testId, [FromQuery] int attemptId)
        {
            var test = await _testsRepo.GetTestAsync(testId);
            if (test is null) return _responseFactory.NotFound(this);

            var creator = await _userManager.GetUserAsync(User);
            if (!test.Creator!.Equals(creator)) return _responseFactory.Forbid(this);

            var attempt = test.PassingInfos
                .SingleOrDefault(i => i.Attempts
                    .Any(a => a.AttemptId == attemptId))
                ?.Attempts.SingleOrDefault(a => a.AttemptId == attemptId);

            if(attempt is null) return _responseFactory.NotFound(this);

            var model = attempt.ToDetailedTest();
            return _responseFactory.Ok(this, model);
        }

        [HttpGet("/api/profile/get-stats")]
        public async Task<IActionResult> GetUserStats([FromQuery] int id)
        {
            var test = await _testsRepo.GetTestAsync(id);
            if (test is null) return _responseFactory.NotFound(this);

            var creator = await _userManager.GetUserAsync(User);
            if (!test.Creator!.Equals(creator)) return _responseFactory.Forbid(this);

            var infos = test.PassingInfos.ToList();
            if (!infos.Any()) return _responseFactory.NoContent(this);

            var summaries = infos
                .Select(i => i.Attempts
                    .Where(a => a.IsEnded)
                    .OrderBy(a => a.TimeEnded)
                    .LastOrDefault())
                .Select(a => a!.ToStatSummary());

            if (!summaries.Any()) return _responseFactory.NoContent(this);

            return _responseFactory.Ok(this, summaries);
        }

        [HttpGet("/api/profile/user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            return _responseFactory.Ok(this, user.ToJsonModel());
        }

        [HttpPost("/api/profile/update-user")]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserRequestModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                    return _responseFactory.BadRequest(this, new[] { "Вы ввели неверный пароль" } );

                user.UserName = model.UserName;
                if (user.Email != model.Email) user.EmailConfirmed = false;
                user.Email = model.Email;
                await _userManager.UpdateAsync(user);

                return _responseFactory.Ok(this);
            }
            else return _responseFactory.BadRequest(this, ModelState.ToStringEnumerable());
        }

        [HttpPost("/api/profile/change-password")]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordRequestModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (result.Succeeded) return _responseFactory.Ok(this);
                return _responseFactory.BadRequest(this, new[] { "Вы ввели неверный пароль" });
            }
            return _responseFactory.BadRequest(this, ModelState.ToStringEnumerable());
        }

        [HttpGet("/api/profile/user-tests")]
        public async Task<IActionResult> GetUserTests()
        {
            var creator = await _userManager.GetUserAsync(User);

            var tests = _testsRepo.GetUserTests(creator).Select(t => t.ToSummary());

            if (!tests.Any()) return _responseFactory.NoContent(this);

            return _responseFactory.Ok(this, tests );
        }

        [HttpGet("/api/profile/passed-tests-info")]
        public async Task<IActionResult> GetPassedTests()
        {
            var user = await _userManager.GetUserAsync(User);
            var infos = _passingInfoRepo.GetUserInfos(user).ToList();
        
            if (!infos.Any()) return _responseFactory.NoContent(this);

            var summaries = infos.Select(i => {
                var model = i.ToPassedTestSummary();
                model.UserRate = i.Test!.Rates.SingleOrDefault(r => r.User!.Equals(user))?.Value ?? -1;
                return model;
            });

            return _responseFactory.Ok(this, summaries);   
        }

        [HttpGet("/api/profile/detailed-test")]
        public async Task<IActionResult> GetDetailedTest([FromQuery] int testId)
        {
            var test = await _testsRepo.GetTestAsync(testId);
            if (test is null) return _responseFactory.NotFound(this);

            var user = await _userManager.GetUserAsync(User);

            var info = await _passingInfoRepo.GetInfoAsync(user, test);
            if (info is null) return _responseFactory.Conflict(this);

            var lastAttempt = info.Attempts.OrderBy(a => a.TimeEnded).LastOrDefault();
            if (lastAttempt is null) return _responseFactory.Conflict(this);

            var model = lastAttempt.ToDetailedTest();

            return _responseFactory.Ok(this, model);
        }

        [HttpPost("/api/profile/check-test")]
        public async Task<IActionResult> CheckTest([FromForm] CheckTestRequestModel model)
        {
            var test = await _testsRepo.GetTestAsync(model.TestId);
            if (test is null) return _responseFactory.NotFound(this);

            var user = await _userManager.GetUserAsync(User);
            if (!user.Equals(test.Creator)) return _responseFactory.Forbid(this);

            var attempt = test.PassingInfos
                .FirstOrDefault(i => i.Attempts.Any(a => a.AttemptId == model.AttemptId))
                ?.Attempts.Single(a => a.AttemptId == model.AttemptId);

            if (attempt is null) return _responseFactory.NotFound(this);
            if (!attempt.IsEnded) return _responseFactory.Conflict(this);

            foreach (var number in model.CorrectAQNumbers)
                attempt.UserAnswers.First(a => a.QuestionNumber == number).IsCorrect = true;
            
            foreach (var number in model.IncorrectAQNumbers)
                attempt.UserAnswers.First(a => a.QuestionNumber == number).IsCorrect = false;

            attempt.CheckInfo!.IsChecked = true;
            await _testsRepo.UpdateTestAsync(test);

            return _responseFactory.Ok(this);
        }
    }
}