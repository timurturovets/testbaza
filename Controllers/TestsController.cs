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
        public async Task<IActionResult> GetTest([FromRoute]int id)
        {
            Test? test = _testsRepo.GetTest(id);
            User creator = await _userManager.GetUserAsync(User);

            if (test is null) return NotFound();

            if (!test.Creator!.Equals(creator)) return Forbid();

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
                    User creator = await _userManager.GetUserAsync(User);

                    if (_testsRepo.GetTest(model.TestName!) is not null)
                    {
                        ViewData["Error"] = "Тест с таким названием уже существует.";
                        return View();
                    }
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
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            Test? test = _testsRepo.GetTest(testId: id);
            User creator = await _userManager.GetUserAsync(User);
            if(test is null)
            {
                ViewData["Error"] = $"Ошибка. Теста с ID {id} не существует. Попробуйте перезайти на страницу редактирования";
                return RedirectToAction(actionName: "index", controllerName: "home");
            }
            if (test.Creator is null)
            {
                ViewData["Error"] = $"Произошла неизвестная ошибка. Попробуйте перезайти на страницу редактирования";
                return RedirectToAction(actionName: "index", controllerName: "home");
            }
            if (!test.Creator.Equals(creator)) return Forbid();
            ViewData["TestId"] = id;
            return View();
        }

        
        
        [HttpPut("/tests/update-test")]
        public async Task<IActionResult> UpdateTest([FromForm] UpdateTestRequestModel model)
        {
            _logger.LogInformation($"New change test request, testName: {model.TestName}, description: {model.Description}," +
                $"IsPrivate: {model.IsPrivate}");

            Test? test = _testsRepo.GetTest(model.TestId);
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

        [HttpPut("/tests/add-question")]
        public async Task<IActionResult> AddQuestion([FromForm] int testId)
        {
            _logger.LogInformation($"New add question request, testId: {testId}");
            Test? test = _testsRepo.GetTest(testId);
            if (test is null) return NotFound();

            User creator = await _userManager.GetUserAsync(User);
            if (!test.Creator!.Equals(creator)) return Forbid();

            int number = test.Questions.Count()+1;
            Question newQuestion = new()
            {
                Test = test,
                Number = number
            };
            _qsRepo.AddQuestion(newQuestion);
            Question? createdQuestion = _qsRepo.GetQuestionByTestAndNumber(test, number);
            int questionId = createdQuestion!.QuestionId;
            return Ok(new { questionId, number });
        }

        [HttpPut("/tests/update-question")]
        public async Task<IActionResult> UpdateQuestion([FromForm] UpdateQuestionRequestModel model)
        {
            _logger.LogError($"New change question request, value: {model.Value}, answer: {model.Answer}, aType: {model.AnswerType}");

            Question? question = _qsRepo.GetQuestion(model.QuestionId);
            if (question is null) return NotFound();

            User creator = await _userManager.GetUserAsync(User);
            if (!question.Test!.Creator!.Equals(creator)) return Forbid();

            question.Value = model.Value;
            question.AnswerType = model.AnswerType;
            question.Answer = model.Answer;
            if (model.Answers is not null)
            {
                model.Answers.ToList().ForEach(a =>
                {
                    Answer? answer = question.MultipleAnswers.FirstOrDefault(ans => ans.AnswerId == a.AnswerId);
                    if (answer is null) return;
                    answer.Value = a.Value;
                });
            }
            _qsRepo.UpdateQuestion(question);

            return Ok();
        }
        [HttpPost("/tests/delete-question")]
        public async Task<IActionResult> DeleteQuestion([FromForm] int questionId)
        {
            _logger.LogInformation($"New delete question request, questionId: {questionId}");

            User creator = await _userManager.GetUserAsync(User);
            Question? question = _qsRepo.GetQuestion(questionId);

            if (question is null) return NotFound();
            if (!question.Test!.Creator!.Equals(creator)) return Forbid();

            _qsRepo.DeleteQuestion(question);
            return Ok();
        }

        [HttpPost("/tests/add-answer")]
        public async Task<IActionResult> AddAnswer(int questionId)
        {
            _logger.LogError($"QuestionId: {questionId}");
            Question? question = _qsRepo.GetQuestion(questionId);
            User creator = await _userManager.GetUserAsync(User);

            if (question is null) return NotFound();
            if (!question.Test!.Creator!.Equals(creator)) return Forbid();

            (int answerId, int number) = _qsRepo.AddAnswerToQuestion(question);
            return Ok(new { answerId , number  });
        }

        [HttpPost("/tests/delete-answer")]
        public async Task<IActionResult> DeleteAnswer(int answerId, int questionId)
        {
            _logger.LogError($"AnswerID: {answerId}");
            Question? question = _qsRepo.GetQuestion(questionId);
            if (question is null) return NotFound();

            User creator = await _userManager.GetUserAsync(User);
            if (!question.Test!.Creator!.Equals(creator)) return Forbid();

            Answer? answer = question.MultipleAnswers.SingleOrDefault(a => a.AnswerId == answerId);
            if (answer is null) return BadRequest();

            _qsRepo.RemoveAnswerFromQuestion(question, answer);
            return Ok();
            
        }
    }
}