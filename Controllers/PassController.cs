using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Factories;
using TestBaza.Repositories;

namespace TestBaza.Controllers
{
    [Authorize]
    public class PassController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ITestsRepository _testsRepo;
        private readonly IPassingInfoRepository _passingInfoRepo;
        private readonly IResponseFactory _responseFactory;
        public PassController(
            UserManager<User> userManager,
            ITestsRepository testsRepo,
            IPassingInfoRepository passingInfoRepo,
            IResponseFactory responseFactory
            )
        {
            _userManager = userManager;
            _testsRepo = testsRepo;
            _passingInfoRepo = passingInfoRepo;
            _responseFactory = responseFactory;
        }

        [HttpGet("/api/pass/info")]
        public async Task<IActionResult> GetInfo([FromQuery] int id)
        {
            Test? test = await _testsRepo.GetTestAsync(id);
            if (test is null) return _responseFactory.NotFound(this);
            if (!test.IsBrowsable) return _responseFactory.Forbid(this);

            TestJsonModel model = test.ToJsonModel(includeAnswers: false);
            return _responseFactory.Ok(this, result: model);
        }
        [HttpPost("/api/pass/start-passing")]
        public async Task<IActionResult> StartPassing([FromForm] StartPassingRequestModel model)
        {
            Test? test = await _testsRepo.GetTestAsync(model.TestId);
            if (test is null) return _responseFactory.NotFound(this);

            User user = await _userManager.GetUserAsync(User);

            PassingInfo? info = await _passingInfoRepo.GetInfoAsync(user, test);

            if (info is null)
            {
                info = new()
                {
                    Test = test,
                    User = user,
                };
                info.Attempts.ToList().Add(new Attempt
                {
                    PassingInfo = info,
                    TimeStarted = DateTime.Now
                });
                await _passingInfoRepo.AddInfoAsync(info);
                return _responseFactory.Ok(this);
            }
            else
            {
                if(test.AllowedAttempts >= info.Attempts.Count())
                {
                    return _responseFactory.Conflict(this);
                }
                else
                {
                    info.Attempts.ToList().Add(new Attempt
                    {
                        PassingInfo = info,
                        TimeStarted = DateTime.Now
                    });
                    await _passingInfoRepo.UpdateInfoAsync(info);
                    return _responseFactory.Ok(this);
                }
            }
        }
    }
}