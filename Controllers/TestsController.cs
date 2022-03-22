using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Factories;
using TestBaza.Extensions;
using TestBaza.Repositories;

namespace TestBaza.Controllers
{
    [Authorize]
    public class TestsController : Controller
    {
        private readonly IRatesRepository _ratesRepo;
        private readonly ITestsRepository _testsRepo;
        private readonly IQuestionsRepository _qsRepo;
        private readonly UserManager<User> _userManager;
        private readonly ITestFactory _testFactory;
        private readonly ILogger<TestsController> _logger;
        public TestsController(
            IRatesRepository ratesRepo,
            ITestsRepository testsRepo,
            IQuestionsRepository qsRepo,
            UserManager<User> userManager,
            ITestFactory testFactory,
            ILogger<TestsController> logger
            )
        {
            _ratesRepo = ratesRepo;
            _testsRepo = testsRepo;
            _qsRepo = qsRepo;
            _userManager = userManager;
            _testFactory = testFactory;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/api/tests/all")]
        public IActionResult All()
        {
            var tests = _testsRepo.GetBrowsableTests();
            if (!tests.Any()) return NoContent();
            IEnumerable<TestSummary> testsSummaries = tests.Select(t => t.ToSummary());
            return Ok(new { tests = testsSummaries });
        }

        [HttpGet]
        public IActionResult Pass([FromQuery] int id)
        {
            Test? test = _testsRepo.GetTest(id);

            if (test is null) return NotFound();

            if (!(test.IsPublished && test.IsBrowsable)) return Forbid();

            ViewData["TestId"] = id;
            return View();

        }

        [HttpGet("/api/tests/wq/get-test{id}")]
        public async Task<IActionResult> GetTest([FromRoute] int id)
        {
            string apiKey = HttpContext.GetApiKey();
            string? clientApiKey = HttpContext.Session.GetString(API_KEY);
            if (apiKey != clientApiKey) return Forbid();

            Test? test = _testsRepo.GetTest(id);
            User creator = await _userManager.GetUserAsync(User);

            if (test is null) return NotFound();

            if (!test.Creator!.Equals(creator)) return Forbid();

            TestJsonModel model = test.ToJsonModel();
            return Ok(model);
        }

        [HttpGet("/api/tests/pass-test-info${id}")]
        public IActionResult GetTestForPass([FromRoute] int id)
        {
            Test? test = _testsRepo.GetTest(id);

            if (test is null) return NotFound();

            TestJsonModel model = test.ToJsonModel();

            model.Questions = model.Questions.Select(q =>
            { 
                q.Answer = string.Empty;
                q.Answers = Array.Empty<AnswerJsonModel>(); 
                return q; 
            }).ToArray();

            return Ok(new { result = model });
        }

        [HttpGet]
        public IActionResult Create() => View();
        
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
                    Test test = _testFactory.Create(
                        testName: model.TestName!,
                        description: model.Description ?? "Без описания",
                        isPrivate: model.IsPrivate,
                        isTimeLimited: model.TimeInfo.IsTimeLimited,
                        timeLimit: model.TimeInfo.ConvertToSeconds(),
                        creator: creator);

                    _testsRepo.AddTest(test);

                    await _userManager.UpdateAsync(creator);
                    int id = _testsRepo.GetTest(test.TestName!)!.TestId;

                    return RedirectToAction(actionName: "edit", controllerName: "tests", new { id });
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
            if (test is null)
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

            string apiKey = HttpContext.GetApiKey();
            ISession session = HttpContext.Session;
            session.SetString(API_KEY, apiKey);

            _logger.LogError($"String from session: {session.GetString(API_KEY)}");
            return View();
        }

        [HttpPut("/api/tests/wq/update-test")]
        public async Task<IActionResult> UpdateTest([FromForm] UpdateTestRequestModel model)
        {
            _logger.LogInformation($"New change test request, testName: {model.TestName}, description: {model.Description}," +
                $"IsPrivate: {model.IsPrivate}");
            if (ModelState.IsValid)
            {
                Test? test = _testsRepo.GetTest(model.TestId);
                if (test is null) return NotFound();

                User creator = await _userManager.GetUserAsync(User);
                if (!test.Creator!.Equals(creator)) return Forbid();

                test.TestName = model.TestName;
                test.Description = model.Description;
                test.IsPrivate = model.IsPrivate;
                test.IsTimeLimited = model.TimeInfo.IsTimeLimited;
                test.TimeLimit = model.TimeInfo.ConvertToSeconds();

                _testsRepo.UpdateTest(test);
                TestJsonModel testModel = test.ToJsonModel();
                return Ok(testModel);
            }
            else return BadRequest();
        }

        [HttpPost("/api/tests/delete-test")]
        public async Task<IActionResult> DeleteTest([FromForm] int testId)
        {
            User creator = await _userManager.GetUserAsync(User);

            Test? test = _testsRepo.GetTest(testId);
            if (test is null) return NotFound();

            if (!test.Creator!.Equals(creator)) return Forbid();

            _testsRepo.RemoveTest(test);
            return Ok();
        }

        [HttpPut("/api/tests/add-question")]
        public async Task<IActionResult> AddQuestion([FromForm] int testId)
        {
            _logger.LogInformation($"New add question request, testId: {testId}");
            Test? test = _testsRepo.GetTest(testId);
            if (test is null) return NotFound();

            User creator = await _userManager.GetUserAsync(User);
            if (!test.Creator!.Equals(creator)) return Forbid();

            int number = test.Questions.Count() + 1;
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

        [HttpPut("/api/tests/update-question")]
        public async Task<IActionResult> UpdateQuestion([FromForm] UpdateQuestionRequestModel model)
        {
            _logger.LogError($"New change question request, value: {model.Value}, answer: {model.Answer}, aType: {model.AnswerType}");

            Question? question = _qsRepo.GetQuestion(model.QuestionId);
            if (question is null) return NotFound();

            User creator = await _userManager.GetUserAsync(User);
            if (!question.Test!.Creator!.Equals(creator)) return Forbid();

            question.Value = model.Value;
            question.Hint = model.Hint;
            question.HintEnabled = model.HintEnabled;
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
            question.AnswerType = model.AnswerType;
            _qsRepo.UpdateQuestion(question);

            return Ok();
        }
        [HttpPost("/api/tests/delete-question")]
        public async Task<IActionResult> DeleteQuestion([FromForm] int questionId)
        {
            _logger.LogInformation($"New delete question request, questionId: {questionId}");

            Question? question = _qsRepo.GetQuestion(questionId);
            if (question is null) return NotFound();

            User creator = await _userManager.GetUserAsync(User);
            if (!question.Test!.Creator!.Equals(creator)) return Forbid();

            _qsRepo.DeleteQuestion(question);
            return Ok();
        }

        [HttpPost("/api/tests/add-answer")]
        public async Task<IActionResult> AddAnswer(int questionId)
        {
            _logger.LogError($"QuestionId: {questionId}");
            Question? question = _qsRepo.GetQuestion(questionId);
            User creator = await _userManager.GetUserAsync(User);

            if (question is null) return NotFound();
            if (!question.Test!.Creator!.Equals(creator)) return Forbid();

            AnswerInfo info = _qsRepo.AddAnswerToQuestion(question);
            return Ok(new { answerId = info.Id, number = info.Number });
        }

        [HttpPost("/api/tests/delete-answer")]
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

        [HttpGet("/api/tests/publish-test{testId}")]
        public async Task<IActionResult> PublishTest([FromRoute] int testId)
        {
            Test? test = _testsRepo.GetTest(testId);
            List<string> errors = new();

            if (test is null)
            {
                errors.Add("Произошла непредвиденная ошибка. Перезагрузите страницу.");
                return BadRequest(new { errors });
            }

            User creator = await _userManager.GetUserAsync(User);
            if (!test.Creator!.Equals(creator)) return Forbid();

            if (string.IsNullOrEmpty(test.TestName))
            {
                errors.Add("Вы не ввели название теста.");
            }
            else if (test.TestName.Length < 4) errors.Add("Название теста должно содержать не менее 4 символов.");

            if (string.IsNullOrEmpty(test.Description)) test.Description = "Без описания.";
            else if (test.Description.Length > 100) errors.Add("Описание теста должно содержать не более 100 символов.");

            if (test.Questions.Count() < 3)
            {
                errors.Add("В тесте должно быть не менее 3 вопросов. ");
            }
            if (test.IsTimeLimited && test.TimeLimit < 30)
            {
                errors.Add("Ограничение по времени прохождения теста должно быть не менее 30 секунд.");
            }
            string noQuestionValueErrors = "Вы не ввели текст ",
                noHintValueErrors = "Вы не ввели текст подсказки ",
                noQuestionAnswerErrors = "Вы не ввели ответ ",
                notAllAnswersEnteredErrors = "Вы ввели не все варианты ответа ",
                answerTypeNotDeclaredErrors = "Вы не выбрали вариант ответа для ",
                notEnoughAnswersErrors = "Вопросы с несколькими вариантами ответа должны иметь минимум 2 ответа. Под этот критерий не проходит ";


            bool hasQuestionsWithoutValue = false,
                hasHintsWithoutValue = false,
                hasQuestionsWithoutAnswer = false,
                hasAnswersWithoutValue = false,
                hasUndeclaredAnswerTypes = false,
                hasQuestionsWithNotEnoughAnswers = false;

            foreach (Question q in test.Questions)
            {
                IEnumerable<Question> qsWithSameNumbers = test.Questions.Where(qn => qn.Number == q.Number);
                int qsCount = qsWithSameNumbers.Count();
                if (qsCount > 1)
                {
                    bool hasQsWithSameNumbers = true;
                    while (hasQsWithSameNumbers)
                    {
                        for (int i = 0; i < qsCount; i++)
                        {
                            Question last = qsWithSameNumbers.Last();
                            qsWithSameNumbers.First(qn => qn.Number == last.Number + 1).Number++;
                            last.Number++;
                        }
                    }
                    if (qsWithSameNumbers.Count() < 2)
                    {
                        hasQsWithSameNumbers = false;
                        _qsRepo.UpdateQuestion(q);
                    }
                }

                int n = q.Number;
                if (string.IsNullOrEmpty(q.Value))
                {
                    hasQuestionsWithoutValue = true;
                    noQuestionValueErrors += $"вопроса {n}, ";
                }
                if (q.HintEnabled && string.IsNullOrEmpty(q.Hint))
                {
                    hasHintsWithoutValue = true;
                    noHintValueErrors += $"вопроса {n}, ";
                }

                if (q.AnswerType == AnswerType.HasToBeTyped && string.IsNullOrEmpty(q.Answer))
                {
                    hasQuestionsWithoutAnswer = true;
                    noQuestionAnswerErrors += $"на вопрос {n}, ";
                }

                if (q.AnswerType == AnswerType.MultipleVariants)
                {
                    foreach (Answer a in q.MultipleAnswers)
                    {
                        IEnumerable<Answer> answersWithSameNumber = q.MultipleAnswers.Where(ans => ans.Number == a.Number);
                        int answersCount = answersWithSameNumber.Count();
                        if (answersCount > 1)
                        {
                            bool hasAnswersWithSameNumbers = true;
                            while (hasAnswersWithSameNumbers)
                            {
                                for (int i = 0; i < answersCount; i++)
                                {
                                    Answer last = answersWithSameNumber.Last();
                                    answersWithSameNumber.First(a => a.Number == last.Number + 1).Number++;
                                    last.Number++;
                                }
                                if (answersWithSameNumber.Count() < 2)
                                {
                                    _qsRepo.UpdateQuestion(q);
                                    hasAnswersWithSameNumbers = false;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(a.Value))
                        {
                            hasAnswersWithoutValue = true;
                            notAllAnswersEnteredErrors += $"на вопрос {n}, ";
                            break;
                        }
                    }
                    if (q.MultipleAnswers.Count() < 2)
                    {
                        hasQuestionsWithNotEnoughAnswers = true;
                        notEnoughAnswersErrors += $"вопрос {n}, ";
                    }
                }
                else if (q.AnswerType != AnswerType.HasToBeTyped)
                {
                    hasUndeclaredAnswerTypes = true;
                    answerTypeNotDeclaredErrors += $"вопроса {n}, ";
                }
            }

            static string replaceLastComma(string value) => Regex.Replace(value, @",\s$", ".");

            string msg;
            if (hasQuestionsWithoutValue)
            {
                msg = replaceLastComma(noQuestionValueErrors);
                errors.Add(msg);
            }
            if (hasHintsWithoutValue)
            {
                msg = replaceLastComma(noHintValueErrors);
                errors.Add(msg);
            }

            if (hasQuestionsWithoutAnswer)
            {     
                msg = replaceLastComma(noQuestionAnswerErrors);
                errors.Add(msg);
            }

            if (hasAnswersWithoutValue)
            {
                msg = replaceLastComma(notAllAnswersEnteredErrors);
                errors.Add(msg);
            }

            if (hasQuestionsWithNotEnoughAnswers)
            {
                msg = replaceLastComma(notEnoughAnswersErrors);
                errors.Add(msg);
            }

            if (hasUndeclaredAnswerTypes)
            {
                msg = replaceLastComma(answerTypeNotDeclaredErrors);
                errors.Add(msg);
            }

            if (errors.Count > 0) return BadRequest(new { errors });


            test.IsPublished = true;
            _testsRepo.UpdateTest(test);

            return Ok();
        }

        [HttpPost("/api/tests/rate-test")]
        public async Task<IActionResult> RateTest([FromForm] RateTestRequestModel model)
        {
            User rater = await _userManager.GetUserAsync(User);

            Test? test = _testsRepo.GetTest(model.TestId);
            if (test is null) return NotFound();

            Rate rate = new() { Value = model.Rate, Test = test, User = rater };
            _ratesRepo.AddRate(rate);
            
            return Ok();
        }
    }
}