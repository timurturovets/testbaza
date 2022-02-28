using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestsBaza.Repositories;

namespace TestBaza.Controllers
{   
    [Authorize]
    [ApiController]
    [Route("api/test")]
    public class TestsController : ControllerBase
    {
        private readonly ITestsRepository _testsRepo;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<TestsController> _logger;
        public TestsController(ITestsRepository testsRepo, 
            UserManager<User> userManager, 
            SignInManager<User> signInManager,
            ILogger<TestsController> logger)
        {
            _testsRepo = testsRepo;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet("get-all-tests")]
        public IActionResult Get()
        {
            if (!_testsRepo.GetAllTests().Any()) return NotFound();
            IEnumerable<TestJsonModel> allTests = _testsRepo.GetAllTests().Select(t=>new TestJsonModel
            {
                TestName = t.TestName!,
                AuthorName=t.Creator!.UserName,
                Questions = t.Questions.Select(q=>new QuestionJsonModel
                {
                    Question = q.Value!,
                    Answer = q.Answer!
                })
            });
            return Ok(allTests);
        }
        

        [HttpGet("get-test/{testId?}")]
        public IActionResult GetTest([FromQuery][FromRoute]int testId)
        {
            Test? test = _testsRepo.GetTest(testId);
            if (test is null) return NotFound(new { msg = $"Теста с идентификатором {testId} не существует"});
            return Ok(test);
        }

        [HttpPost("get-test")]
        public IActionResult GetTest([FromBody][FromForm]string testName)
        {
            Test? test = _testsRepo.GetTest(testName);
            if (test is null) return NotFound();
            TestJsonModel model = new()
            {
                AuthorName = test.Creator!.UserName,
                TestName = test.TestName,
                Questions = test.Questions.Select(q => new QuestionJsonModel
                {
                    Question = q.Value!,
                    Answer = q.Answer!
                })
            };
            return Ok(new { test = model });
        }

        
        [HttpPost("add-test")]
        public async Task<IActionResult> CreateTest([FromForm] CreateTestRequestModel model)
        {
            try
            {
                _logger.LogInformation($"New create test request, TestName-{model.TestName}, IsPrivate:{model.IsPrivate}");
                if (ModelState.IsValid)
                {
                    User creator = await _userManager.GetUserAsync(HttpContext.User);

                    if (_testsRepo.GetTest(model.TestName!) is not null)
                        return Forbid();

                    Test test = new()
                    {
                        Creator = creator,
                        TestName = model.TestName!,
                        IsPrivate = model.IsPrivate,
                        TimeCreated = DateTime.Now
                    };
                    _testsRepo.AddTest(test);
                    return Ok();
                }
                else
                {
                    var errors = ModelState.Select(entry =>
                    {
                        var query = entry.Value?.Errors.Select(e => e.ErrorMessage);
                        if (query is null || !query.Any()) return string.Empty;
                        else return query.Aggregate((x, y) => x + ", " + y);
                    });
                    return BadRequest(new { errors });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.InnerException?.Message ?? e.Message);
                return BadRequest(105);
            }
        }

        [HttpPost("get-users-tests")]
        public async Task<IActionResult> GetUsersTests([FromForm] string stuff)
        {

            User creator = await _userManager.GetUserAsync(HttpContext.User);

            if (creator is null) return Unauthorized();
            if (!creator.Tests.Any()) return NotFound(new { result = "emptytests" });
            IEnumerable<TestJsonModel> tests = creator.Tests.Select(t => new TestJsonModel
            {
                TestName = t.TestName!,
                AuthorName = creator.UserName,
                Questions = t.Questions.Select(q => new QuestionJsonModel
                {
                    Question = q.Value!,
                    Answer = q.Answer!
                })
            });
            return Ok(tests);
        }

        [HttpPut("update-test")]
        public async Task<IActionResult> UpdateTest([FromForm] UpdateTestRequestModel model)
        {
            User? creator = await _userManager.FindByNameAsync(User.Identity!.Name!);
            Test? test = _testsRepo.GetTest(model.TestId);

            if (test is null) return NotFound(new { message = $"Теста с идентификатором {model.TestId} не существует" });
            if (creator is null || !test.Creator!.Equals(creator)) return Unauthorized();

            test.TestName = model.NewTestName ?? test.TestName;
            test.Questions = test.Questions.Concat(model.NewQuestions.Select(q=>new Question 
            {
                Test = test
            }));

            _testsRepo.UpdateTest(test);
            return Ok();
        }

        [HttpDelete("remove-test")]
        public async Task<IActionResult> RemoveTest([FromBody][FromForm] int testId)
        {
            User? creator = await _userManager.GetUserAsync(HttpContext.User);
            Test? test = _testsRepo.GetTest(testId);

            if (test is null) return NotFound(new { message = $"Теста с идентификатором {testId} не существует" });
            if (creator is null || !test.Creator!.Equals(creator)) return Unauthorized();
                  _testsRepo.RemoveTest(test);
            return Ok();
        }
    }
}