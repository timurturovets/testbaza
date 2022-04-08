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
        private readonly ILogger<PassController> _logger;
        public PassController(
            UserManager<User> userManager,
            ITestsRepository testsRepo,
            IPassingInfoRepository passingInfoRepo,
<<<<<<< HEAD
            ILogger<PassController> logger,
=======
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
            IResponseFactory responseFactory
            )
        {
            _userManager = userManager;
<<<<<<< HEAD
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   _testsRepo = testsRepo;
            _passingInfoRepo = passingInfoRepo;
            _logger = logger;
=======
            _testsRepo = testsRepo;
            _passingInfoRepo = passingInfoRepo;
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
            _responseFactory = responseFactory;
        }

        [HttpGet("/api/pass/info")]
        public async Task<IActionResult> GetInfo([FromQuery] int id)
        {
            Test? test = await _testsRepo.GetTestAsync(id);
            if (test is null) return _responseFactory.NotFound(this);
            if (!test.IsBrowsable) return _responseFactory.Forbid(this);

            User user = await _userManager.GetUserAsync(User);
            PassingInfo? info = (await _passingInfoRepo.GetInfoAsync(user, test))!;
            TestJsonModel testModel = test.ToJsonModel(includeAnswers: false);

            if(info is null)
            {
                info = new() { User = user, Test = test };
                await _passingInfoRepo.AddInfoAsync(info);
<<<<<<< HEAD
                return _responseFactory.StatusCode(this, 201, 
                    result: new { test = testModel, attemptsLeft = test.AllowedAttempts });
            }

            Attempt? currentAttempt = info.Attempts.SingleOrDefault(a => !a.IsEnded);
            _logger.LogError($"Attempt is null: {currentAttempt is null}");

            if (test.AreAttemptsLimited 
                && info.Attempts.Count() >= test.AllowedAttempts
                && currentAttempt is null)
=======
                return _responseFactory.StatusCode(this, 100, result: testModel);
            }
            if(test.AreAttemptsLimited && info!.Attempts.Count() >= test.AllowedAttempts)
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
            {
                return _responseFactory.Conflict(this);
            }

<<<<<<< HEAD
            int attemptsLeft = test.AreAttemptsLimited
           ? test.AllowedAttempts - info.Attempts.Count()
           : -1;

            if (currentAttempt is not null)
            {                                
=======
            Attempt? currentAttempt = info.Attempts.SingleOrDefault(a => a.TimeEnded == default);
            if (currentAttempt is not null)
            {
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
                AttemptJsonModel attemptModel = currentAttempt.ToJsonModel();
                if (test.IsTimeLimited)
                {
                    DateTime end = currentAttempt.TimeStarted + TimeSpan.FromSeconds(test.TimeLimit);
                    if (end < DateTime.Now)
                    {
                        currentAttempt.TimeEnded = end;
<<<<<<< HEAD
                        currentAttempt.IsEnded = true;
                        await _passingInfoRepo.UpdateInfoAsync(info);

                        return _responseFactory.StatusCode(this, 201, result: new { test = testModel, attemptsLeft});
=======
                        await _passingInfoRepo.UpdateInfoAsync(info);
                        return _responseFactory.StatusCode(this, 100, result: attemptModel);
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
                    }
                }
                return _responseFactory.Ok(this, result: attemptModel);
            }
<<<<<<< HEAD
            return _responseFactory.StatusCode(this, 201, result: new { test = testModel, attemptsLeft });
        }

        [HttpGet("/api/pass/start-passing")]
        public async Task<IActionResult> StartPassing([FromQuery] int testId)
=======

            return _responseFactory.Ok(this, result: testModel);

        }

        [HttpPost("/api/pass/start-passing")]
        public async Task<IActionResult> StartPassing([FromForm] int testId)
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
        {
            Test? test = await _testsRepo.GetTestAsync(testId);
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
<<<<<<< HEAD
                if (test.AllowedAttempts <= info.Attempts.Count())
=======
                if(test.AllowedAttempts <= info.Attempts.Count())
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
                {
                    return _responseFactory.Conflict(this);
                }
                else
                {
<<<<<<< HEAD
                    var attempts = info.Attempts.ToList();
                    attempts.Add(new Attempt
=======
                    info.Attempts.ToList().Add(new Attempt
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
                    {
                        PassingInfo = info,
                        TimeStarted = DateTime.Now
                    });
<<<<<<< HEAD
                    info.Attempts = attempts;
=======
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
                    await _passingInfoRepo.UpdateInfoAsync(info);
                    return _responseFactory.Ok(this);
                }
            }
<<<<<<< HEAD
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
=======
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
        }
    }
}