using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Repositories;

namespace TestBaza.Controllers
{   
    [Authorize]
    public class TestsController : Controller
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

        public IActionResult Index()
        {
            if (!_signInManager.IsSignedIn(User)) 
                return RedirectToAction(actionName: "login", controllerName: "auth");
            return View();
        }

        [HttpGet("/tests/all")]
        public IActionResult All()
        {
            if (!_testsRepo.GetReadyTests().Any()) return StatusCode(228);
            IEnumerable<TestJsonModel> allTests = _testsRepo.GetReadyTests().Select(t=>new TestJsonModel
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
        
        [HttpGet("/tests/getbyid")]
        public IActionResult GetTest([FromQuery][FromRoute]int testId)
        {
            Test? test = _testsRepo.GetTest(testId);
            if (test is null) return NotFound();
            return Ok(test);
        }

        [HttpGet("/tests/getbyname")]
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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateTestRequestModel model)
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
                        Description = model.Description ?? "Без описания.",
                        IsPrivate = model.IsPrivate,
                        TimeCreated = DateTime.Now
                    };
                    _testsRepo.AddTest(test);
                    int id = _testsRepo.GetTest(test.TestName)!.TestId;
                    return RedirectToAction(actionName:"edit", controllerName:"tests", new { id });
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

        [HttpGet]
        [Route("/tests/edit{id?}")]
        public IActionResult Edit([FromRoute] int id)
        {
            Test? test = _testsRepo.GetTest(testId: id);
            if(test is null)
            {
                ViewData["Error"] = $"Ошибка. Теста с ID {id} не существует. Попробуйте перезайти на страницу редактирования";
                return RedirectToAction(actionName: "index", controllerName: "home");
            }
            return View();
        }
    }
}