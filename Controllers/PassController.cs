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
        private readonly IPassingInfoFactory _passingInfoFactory;
        private readonly IResponseFactory _responseFactory;
        public PassController(
            UserManager<User> userManager,
            ITestsRepository testsRepo,
            IPassingInfoRepository passingInfoRepo,
            IPassingInfoFactory passingInfoFactory,
            IResponseFactory responseFactory
            )
        {
            _userManager = userManager;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   _testsRepo = testsRepo;
            _passingInfoRepo = passingInfoRepo;
            _responseFactory = responseFactory;
            _passingInfoFactory = passingInfoFactory;
        }

        [HttpGet("/api/pass/info")]
        public async Task<IActionResult> GetInfo([FromQuery] int id)
        {
            Test? test = await _testsRepo.GetTestAsync(id);
            if (test is null) return _responseFactory.NotFound(this);
            if (!test.IsPublished) return _responseFactory.Forbid(this);

            User user = await _userManager.GetUserAsync(User);
            PassingInfo? info = (await _passingInfoRepo.GetInfoAsync(user, test))!;
            TestJsonModel testModel = test.ToJsonModel(includeAnswers: false);

            if(info is null)
            {
                info = _passingInfoFactory.Create(user, test);
                await _passingInfoRepo.AddInfoAsync(info);
                return _responseFactory.StatusCode(this, 201, 
                    result: new { test = testModel, attemptsLeft = test.AllowedAttempts });
            }

            Attempt? currentAttempt = info.Attempts.SingleOrDefault(a => !a.IsEnded);

            if (test.AreAttemptsLimited 
                && info.Attempts.Count() >= test.AllowedAttempts
                && currentAttempt is null)
            {
                return _responseFactory.Conflict(this);
            }

            int attemptsLeft = test.AreAttemptsLimited
           ? test.AllowedAttempts - info.Attempts.Count()
           : -1;

            if (currentAttempt is not null)
            {                                
                AttemptJsonModel attemptModel = currentAttempt.ToJsonModel();
                if (test.IsTimeLimited)
                {
                    DateTime end = currentAttempt.TimeStarted + TimeSpan.FromSeconds(test.TimeLimit);
                    if (end < DateTime.Now)
                    {
                        currentAttempt.TimeEnded = end;
                        currentAttempt.IsEnded = true;
                        await _passingInfoRepo.UpdateInfoAsync(info);

                        return _responseFactory.StatusCode(this, 201, result: new { test = testModel, attemptsLeft});
                    }
                }
                return _responseFactory.Ok(this, result: attemptModel);
            }
            return _responseFactory.StatusCode(this, 201, result: new { test = testModel, attemptsLeft });
        }

        [HttpGet("/api/pass/start-passing")]
        public async Task<IActionResult> StartPassing([FromQuery] int testId)
        {
            Test? test = await _testsRepo.GetTestAsync(testId);
            if (test is null) return _responseFactory.NotFound(this);

            User user = await _userManager.GetUserAsync(User);

            PassingInfo? info = await _passingInfoRepo.GetInfoAsync(user, test);

            if (info is null)
            {
                info = _passingInfoFactory.Create(user, test);

                var attempts = info.Attempts.ToList();
                attempts.Add(new Attempt
                {
                    PassingInfo = info,
                    TimeStarted = DateTime.Now
                });
                info.Attempts = attempts;

                await _passingInfoRepo.AddInfoAsync(info);
                return _responseFactory.Ok(this);
            }
            else
            {
                if (test.AreAttemptsLimited && test.AllowedAttempts <= info.Attempts.Count())
                {
                    return _responseFactory.Conflict(this);
                }
                else
                {
                    var attempts = info.Attempts.ToList();
                    attempts.Add(new Attempt
                    {
                        PassingInfo = info,
                        TimeStarted = DateTime.Now
                    });
                    info.Attempts = attempts;
                    await _passingInfoRepo.UpdateInfoAsync(info);
                    return _responseFactory.Ok(this);
                }
            }
        }

        [HttpPost("/api/pass/save-answer")]
        public async Task<IActionResult> SaveAnswer([FromForm] SaveAnswerRequestModel model)
        {
            Test? test = await _testsRepo.GetTestAsync(model.TestId);
            if (test is null) return _responseFactory.NotFound(this);

            User user = await _userManager.GetUserAsync(User);

            PassingInfo? info = await _passingInfoRepo.GetInfoAsync(user, test);
            if (info is null) return _responseFactory.BadRequest(this);

            Attempt? currentAttempt = info.Attempts.SingleOrDefault(a => !a.IsEnded);
            if (currentAttempt is null) return _responseFactory.Conflict(this);

            UserAnswer? answer = currentAttempt.UserAnswers
                .SingleOrDefault(a => a.QuestionNumber == model.QuestionNumber);
            if (answer is null)
            {
                answer = new()
                {
                    Attempt = currentAttempt,
                    QuestionNumber = model.QuestionNumber,
                    Value = model.Value
                };
                var answers = currentAttempt.UserAnswers.ToList();
                answers.Add(answer);
                currentAttempt.UserAnswers = answers;
            }
            else answer.Value = model.Value;

            await _passingInfoRepo.UpdateInfoAsync(info);

            return _responseFactory.Ok(this);
        }

        [HttpGet("/api/pass/end-passing")]
        public async Task<IActionResult> EndPassing([FromQuery] int testId)
        {
            Test? test = await _testsRepo.GetTestAsync(testId);
            if (test is null) return _responseFactory.NotFound(this);

            User user = await _userManager.GetUserAsync(User);

            PassingInfo? info = await _passingInfoRepo.GetInfoAsync(user, test);
            if (info is null) return _responseFactory.Conflict(this);

            Attempt? currentAttempt = info.Attempts.SingleOrDefault(a => !a.IsEnded);
            if (currentAttempt is null) return _responseFactory.Conflict(this);

            currentAttempt.IsEnded = true;
            currentAttempt.TimeEnded = DateTime.Now;
            currentAttempt.CheckInfo = new()
            {
                Attempt = currentAttempt,
                Checker = test.Creator,
                IsChecked = false
            };
            await _passingInfoRepo.UpdateInfoAsync(info);
            return _responseFactory.Ok(this);
        }
    }
}