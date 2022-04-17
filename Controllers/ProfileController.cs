using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Models;
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
        private readonly IRatesRepository _ratesRepo;
        private readonly IResponseFactory _responseFactory;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ProfileController> _logger;
        public ProfileController(
            ITestsRepository testsRepo,
            IPassingInfoRepository passingInfoRepo,
            IRatesRepository ratesRepo,
            IResponseFactory responseFactory,
            UserManager<User> userManager,
            ILogger<ProfileController> logger
            )
        {
            _testsRepo = testsRepo;
            _passingInfoRepo = passingInfoRepo;
            _ratesRepo = ratesRepo;
            _responseFactory = responseFactory;
            _userManager = userManager;
            _logger = logger;
        }

        [Route("/profile")]
        public IActionResult Get() => _responseFactory.View(this, viewName: "main");

        [Route("/profile/user-tests")]
        public IActionResult UserTests() => _responseFactory.View(this);
        [Route("/profile/test-stats{id}")]
        public async Task<IActionResult> TestStats([FromQuery] int id)
        {
            Test? test = await _testsRepo.GetTestAsync(id);
            if (test is null) return _responseFactory.NotFound(this);

            ViewData["TestId"] = id;
            return _responseFactory.View(this);
        }

        [HttpGet("/api/profile/user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            User user = await _userManager.GetUserAsync(User);
            return _responseFactory.Ok(this, result: user.ToJsonModel());
        }

        [HttpPost("/api/profile/update-user")]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserRequestModel model)
        {
            _logger.LogError($"New update user request, name: {model.UserName}, " +
                $"email: {model.Email}, password: {model.Password}");
            if (ModelState.IsValid)
            {
                User user = await _userManager.GetUserAsync(User);
                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                    return _responseFactory.BadRequest(this, result: new[] { "Вы ввели неверный пароль" } );

                user.UserName = model.UserName;
                if (user.Email != model.Email) user.EmailConfirmed = false;
                user.Email = model.Email;
                await _userManager.UpdateAsync(user);

                return _responseFactory.Ok(this);
            }
            else return _responseFactory.BadRequest(this, result: ModelState.ToStringEnumerable());
        }

        [HttpPost("/api/profile/change-password")]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordRequestModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.GetUserAsync(User);

                IdentityResult result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (result.Succeeded) return _responseFactory.Ok(this);
                else return _responseFactory.BadRequest(this, result: new[] { "Вы ввели неверный пароль" });
            }
            else return _responseFactory.BadRequest(this, result: ModelState.ToStringEnumerable());
        }

        [HttpGet("/api/profile/user-tests")]
        public async Task<IActionResult> GetUserTests()
        {
            User creator = await _userManager.GetUserAsync(User);

            IEnumerable<TestSummary> tests = _testsRepo.GetUserTests(creator).Select(t => t.ToSummary());

            if (!tests.Any()) return _responseFactory.NoContent(this);

            return _responseFactory.Ok(this, result: tests );
        }

        [HttpGet("/api/profile/passed-tests-info")]
        public async Task<IActionResult> GetPassedTests()
        {
            User user = await _userManager.GetUserAsync(User);
            _logger.LogCritical("in method");
            IEnumerable<PassingInfo> infos = _passingInfoRepo.GetUserInfos(user);

            if (!infos.Any()) return _responseFactory.NoContent(this);

            IEnumerable<PassedTestSummary> summaries = infos.Select(i => {
                var model = i.ToPassedTestSummary();
                _logger.LogWarning($"i.test.rates.length: {i.Test!.Rates.Count()}");
                model.UserRate = i.Test!.Rates.SingleOrDefault(r => r.User!.Equals(user))?.Value ?? -1;
                return model;
            });

            return _responseFactory.Ok(this, result: summaries);   
        }

        [HttpGet("/api/profile/detailed-test")]
        public async Task<IActionResult> GetDetailedTest([FromQuery] int testId)
        {
            Test? test = await _testsRepo.GetTestAsync(testId);
            if (test is null) return _responseFactory.NotFound(this);

            User user = await _userManager.GetUserAsync(User);

            PassingInfo? info = await _passingInfoRepo.GetInfoAsync(user, test);
            if (info is null) return _responseFactory.Conflict(this);

            Attempt? lastAttempt = info.Attempts.OrderBy(a => a.TimeEnded).LastOrDefault();
            if (lastAttempt is null) return _responseFactory.Conflict(this);

            DetailedPassedTest model = lastAttempt.ToDetailedTest();

            return _responseFactory.Ok(this, result: model);
        }
    }
}