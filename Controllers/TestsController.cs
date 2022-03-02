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
        private readonly IQuestionsRepository _qsRepo;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<TestsController> _logger;
        public TestsController(ITestsRepository testsRepo, 
            IQuestionsRepository qsRepo,
            UserManager<User> userManager, 
            SignInManager<User> signInManager,
            ILogger<TestsController> logger)
        {
            _testsRepo = testsRepo;
            _qsRepo = qsRepo;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/tests/all")]
        public IActionResult All()
        {
            if (!_testsRepo.GetReadyTests().Any()) return StatusCode(228);
            IEnumerable<TestJsonModel> allTests = _testsRepo.GetReadyTests().Select(t => t.ToJsonModel());
            return Ok(allTests);
        }

        [HttpGet("/tests/get-test{id}")]
        public IActionResult GetTest([FromRoute]int id)
        {
            Test? test = _testsRepo.GetTest(id);
            if (test is null) return NotFound();
            TestJsonModel model = test.ToJsonModel();
            return Ok(model);
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
                else return View();
                
            }
            catch (Exception e)
            {
                _logger.LogError(e.InnerException?.Message ?? e.Message);
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("/tests/edit{id}")]
        public IActionResult Edit([FromRoute] int id)
        {
            Test? test = _testsRepo.GetTest(testId: id);
            if(test is null)
            {
                ViewData["Error"] = $"Ошибка. Теста с ID {id} не существует. Попробуйте перезайти на страницу редактирования";
                return RedirectToAction(actionName: "index", controllerName: "home");
            }
            ViewData["TestId"] = id;
            return View();
        }

        [HttpPut("/tests/update-question")]
        public async Task<IActionResult> UpdateQuestion([FromForm] UpdateQuestionRequestModel model)
        {
            _logger.LogInformation($"New change question request, value: {model.Value}, answer: {model.Answer}");

            Question? question = _qsRepo.GetQuestion(model.Id);
            if (question is null) return NotFound();

            User creator = await _userManager.GetUserAsync(User);
            if (!question.Test!.Creator!.Equals(creator)) return Forbid();

            question.Value = model.Value;
            question.Answer = model.Answer;

            _qsRepo.UpdateQuestion(question);

            return Ok();
        }
        
        [HttpPut("/tests/update-test")]
        public async Task<IActionResult> UpdateTest([FromForm] UpdateTestRequestModel model)
        {
            _logger.LogInformation($"New change test request, testName: {model.TestName}, description: {model.Description}," +
                $"IsPrivate: {model.IsPrivate}");

            Test? test = _testsRepo.GetTest(model.Id);
            if (test is null) return NotFound();

            User creator = await _userManager.GetUserAsync(User);
            if (!test.Creator!.Equals(creator)) return Forbid();

            test.TestName = model.TestName;
            test.Description = model.Description;
            test.IsPrivate = model.IsPrivate;

            _testsRepo.UpdateTest(test);
            TestJsonModel testModel = test.ToJsonModel();
            return Ok(testModel);
        }
    }
}