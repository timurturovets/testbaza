using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Factories;
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
        private readonly IResponseFactory _responseFactory;
        public TestsController(
            IRatesRepository ratesRepo,
            ITestsRepository testsRepo,
            IQuestionsRepository qsRepo,

            UserManager<User> userManager,

            ITestFactory testFactory,
            IResponseFactory responseFactory
            )
        {
            _ratesRepo = ratesRepo;
            _testsRepo = testsRepo;
            _qsRepo = qsRepo;

            _userManager = userManager;

            _testFactory = testFactory;
            _responseFactory = responseFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/api/tests/all")]
        public IActionResult All()
        {
            var tests = _testsRepo.GetBrowsableTests();
            var list = tests.ToList();
            if (list.Count == 0) return _responseFactory.NoContent(this);

            var testsSummaries = list.Select(t => t.ToSummary());
            return _responseFactory.Ok(this, testsSummaries);
        }

        [HttpGet]
        public async Task<IActionResult> Pass([FromQuery] int id)
        {
            var test = await _testsRepo.GetTestAsync(id);

            if (test is null) return _responseFactory.NotFound(this);

            if (!(test.IsPublished && test.IsBrowsable)) return _responseFactory.Forbid(this);

            ViewData["TestId"] = id;
            return _responseFactory.View(this);
        }

        [Route("/tests/share")]
        public async Task<IActionResult> PassByLink([FromQuery] string test)
        {
            var passingTest = await _testsRepo.GetTestByLinkAsync(test);
            if (passingTest is null) return _responseFactory.NotFound(this);

            if (!passingTest.IsPublished) return _responseFactory.Forbid(this);

            ViewData["TestId"] = passingTest.TestId;
            return _responseFactory.View(this, "Pass");
        }

        [HttpGet("/api/tests/wq/get-test{id:int}")]
        public async Task<IActionResult> GetTest([FromRoute] int id)
        {
            var test = await _testsRepo.GetTestAsync(id);
            var creator = await _userManager.GetUserAsync(User);

            if (test is null) return _responseFactory.NotFound(this);

            if (!test.Creator!.Equals(creator)) return _responseFactory.Forbid(this);

            var model = test.ToJsonModel();
            return _responseFactory.Ok(this, model);
        }

        [HttpGet]
        public IActionResult Create() => View();
        
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateTestRequestModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var creator = await _userManager.GetUserAsync(User);
                    if (await _testsRepo.GetTestAsync(model.TestName!) is not null)
                    {
                        ViewData["Error"] = "Тест с таким названием уже существует.";
                        return _responseFactory.View(this);
                    }

                    var test = _testFactory.Create(
                        model.TestName!,
                        model.Description ?? "Без описания",
                        model.IsPrivate,
                        model.TimeInfo?.IsTimeLimited ?? false,
                        model.TimeInfo?.ConvertToSeconds() ?? 0,
                        creator);

                    await _testsRepo.AddTestAsync(test);

                    await _userManager.UpdateAsync(creator);
                    var id = test.TestId;

                    return _responseFactory.RedirectToAction(this, "edit", "tests", new { id });
                }
                return View();
            }
            catch 
            {
                return _responseFactory.BadRequest(this);
            }
        }

        [HttpGet]
        [Route("/tests/edit{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var test = await _testsRepo.GetTestAsync(id);
            var creator = await _userManager.GetUserAsync(User);
            if (test is null)
            {
                ViewData["Error"] = $"Ошибка. Теста с ID {id} не существует. Попробуйте перезайти на страницу редактирования";
                return _responseFactory.RedirectToAction(this, "index", "home", null);
            }
            if (test.Creator is null)
            {
                ViewData["Error"] = $"Произошла неизвестная ошибка. Попробуйте перезайти на страницу редактирования";
                return _responseFactory.RedirectToAction(this, "index", "home", null);
            }
            if (!test.Creator.Equals(creator)) return _responseFactory.Forbid(this);
            ViewData["TestId"] = id;

            return _responseFactory.View(this);
        }

        [HttpPut("/api/tests/wq/update-test")]
        public async Task<IActionResult> UpdateTest([FromForm] UpdateTestRequestModel model)
        {if (ModelState.IsValid)
            {
                var test = await _testsRepo.GetTestAsync(model.TestId);
                if (test is null) return _responseFactory.NotFound(this);

                var creator = await _userManager.GetUserAsync(User);
                if (!test.Creator!.Equals(creator)) return _responseFactory.Forbid(this);

                test.Update(model);

                await _testsRepo.UpdateTestAsync(test);

                var testModel = test.ToJsonModel();
                return _responseFactory.Ok(this, testModel);
            }
            else return _responseFactory.BadRequest(this);
        }

        [HttpPost("/api/tests/delete-test")]
        public async Task<IActionResult> DeleteTest([FromForm] int testId)
        {
            var creator = await _userManager.GetUserAsync(User);

            var test = await _testsRepo.GetTestAsync(testId);
            if (test is null) return _responseFactory.NotFound(this);

            if (!test.Creator!.Equals(creator)) return _responseFactory.Forbid(this);

            await _testsRepo.RemoveTestAsync(test);

            return _responseFactory.Ok(this);
        }

        [HttpPut("/api/tests/add-question")]
        public async Task<IActionResult> AddQuestion([FromForm] int testId)
        {
            var test = await _testsRepo.GetTestAsync(testId);
            if (test is null) return _responseFactory.NotFound(this);

            var creator = await _userManager.GetUserAsync(User);
            if (!test.Creator!.Equals(creator)) return _responseFactory.Forbid(this);

            var number = test.Questions.Count() + 1;

            Question newQuestion = new()
            {
                Test = test,
                Number = number
            };

            await _qsRepo.AddQuestionAsync(newQuestion);
            var createdQuestion = _qsRepo.GetQuestion(test, number);
            var questionId = createdQuestion!.QuestionId;

            return _responseFactory.Ok(this, result: new { questionId, number });
        }

        [HttpPut("/api/tests/update-question")]
        public async Task<IActionResult> UpdateQuestion([FromForm] UpdateQuestionRequestModel model)
        {
            var question = await _qsRepo.GetQuestionAsync(model.QuestionId);
            if (question is null) return _responseFactory.NotFound(this);

            var creator = await _userManager.GetUserAsync(User);
            if (!question.Test!.Creator!.Equals(creator)) return _responseFactory.Forbid(this);

            question.Value = model.Value;
            question.Hint = model.Hint;
            question.HintEnabled = model.HintEnabled;
            question.Answer = model.Answer;
            if (model.Answers is not null)
            {
                model.Answers.ToList().ForEach(a =>
                {
                    var answer = question.MultipleAnswers.FirstOrDefault(ans => ans.AnswerId == a.AnswerId);
                    if (answer is null) return;
                    answer.Value = a.Value;
                });
            }
            question.AnswerType = model.AnswerType;
            question.CorrectAnswerNumber = model.CorrectAnswerNumber;

            await _qsRepo.UpdateQuestionAsync(question);

            return _responseFactory.Ok(this);
        }
        [HttpPost("/api/tests/delete-question")]
        public async Task<IActionResult> DeleteQuestion([FromForm] int questionId)
        {
            var question = await _qsRepo.GetQuestionAsync(questionId);
            if (question is null) return _responseFactory.NotFound(this);

            var creator = await _userManager.GetUserAsync(User);
            if (!question.Test!.Creator!.Equals(creator)) return _responseFactory.Forbid(this);
            await _qsRepo.DeleteQuestionAsync(question);
            return _responseFactory.Ok(this);
        }

        [HttpPost("/api/tests/add-answer")]
        public async Task<IActionResult> AddAnswer(int questionId)
        {
            var question = await _qsRepo.GetQuestionAsync(questionId);
            var creator = await _userManager.GetUserAsync(User);

            if (question is null) return _responseFactory.NotFound(this);
            if (!question.Test!.Creator!.Equals(creator)) return _responseFactory.Forbid(this);

            var info = await _qsRepo.AddAnswerToQuestionAsync(question);
            return _responseFactory.Ok(this, result: new { answerId = info.Id, number = info.Number });
        }

        [HttpPost("/api/tests/delete-answer")]
        public async Task<IActionResult> DeleteAnswer(int answerId, int questionId)
        {
            var question = await _qsRepo.GetQuestionAsync(questionId);
            if (question is null) return _responseFactory.NotFound(this);

            var creator = await _userManager.GetUserAsync(User);
            if (!question.Test!.Creator!.Equals(creator)) return _responseFactory.Forbid(this);

            var answer = question.MultipleAnswers.SingleOrDefault(a => a.AnswerId == answerId);
            if (answer is null) return _responseFactory.BadRequest(this);

            await _qsRepo.RemoveAnswerFromQuestionAsync(question, answer);
            return _responseFactory.Ok(this);
        }

        [HttpGet("/api/tests/publish-test{testId}")]
        public async Task<IActionResult> PublishTest([FromRoute] int testId)
        {
            var test = await _testsRepo.GetTestAsync(testId);
            List<string> errors = new();

            if (test is null)
            {
                errors.Add("Произошла непредвиденная ошибка. Перезагрузите страницу.");
                return _responseFactory.BadRequest(this, result: errors );
            }

            var creator = await _userManager.GetUserAsync(User);
            if (!test.Creator!.Equals(creator)) return _responseFactory.Forbid(this);

            if (string.IsNullOrEmpty(test.TestName))
            {
                errors.Add("Вы не ввели название теста.");
            }
            else if (test.TestName.Length < 4) errors.Add("Название теста должно содержать не менее 4 символов.");

            if (string.IsNullOrEmpty(test.Description)) test.Description = "Без описания.";
            else if (test.Description.Length > 100) errors.Add("Описание теста должно содержать не более 100 символов.");

            if (!test.Questions.Any())
            {
                errors.Add("В тесте должен быть хотя бы один вопрос. ");
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
                notEnoughAnswersErrors = "Вопросы с несколькими вариантами ответа должны иметь минимум 2 ответа. Под этот критерий не проходит ",
                correctAnswerUnchosenErrors = "Вы не выбрали верный ответ из нескольких вариантов ответа для ",
                nullAnswerErrors = "Ответ не может иметь значение null. Под этот критерий не проходит ";

            bool hasQuestionsWithoutValue = false,
                hasHintsWithoutValue = false,
                hasQuestionsWithoutAnswer = false,
                hasAnswersWithoutValue = false,
                hasUndeclaredAnswerTypes = false,
                hasQuestionsWithNotEnoughAnswers = false,
                hasUnchosenCorrectAnswers = false,
                hasNullAnswers = false;

            foreach (var q in test.Questions)
            {
                var qsWithSameNumbers = test.Questions.Where(qn => qn.Number == q.Number);
                var withSameNumbers = qsWithSameNumbers as Question[] 
                                        ?? qsWithSameNumbers.ToArray();
                var qsCount = withSameNumbers.Count();
                if (qsCount > 1)
                {
                    while (true)
                    {
                        for (var i = 0; i < qsCount; i++)
                        {
                            var last = withSameNumbers.Last();
                            withSameNumbers.First(qn => qn.Number == last.Number + 1).Number++;
                            last.Number++;
                        }

                        if (withSameNumbers.Count() >= 2) continue;
                        await _qsRepo.UpdateQuestionAsync(q);
                        break;
                    }

                }

                var n = q.Number;
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
                
                if(q.AnswerType == AnswerType.HasToBeTyped && q.Answer == "null")
                {
                    hasNullAnswers = true;
                    nullAnswerErrors += $"вопрос {n}, ";
                }
                if (q.AnswerType == AnswerType.MultipleVariants)
                {
                    var qAnswers = q.MultipleAnswers.ToList();
                    foreach (var a in qAnswers)
                    {
                        var answersWithSameNumber = qAnswers
                            .Where(ans => ans.Number == a.Number).ToList();
                        var answersCount = answersWithSameNumber.Count;
                        if (answersCount > 1)
                        {
                            while (true)
                            {
                                for (var i = 0; i < answersCount; i++)
                                {
                                    var last = answersWithSameNumber.Last();
                                    answersWithSameNumber.First(ans => ans.Number == last.Number + 1).Number++;
                                    qAnswers.First(ans => ans.Number == last.Number + 1).Number++;
                                    last.Number++;
                                }

                                if (answersWithSameNumber.Count >= 2) continue;
                                q.MultipleAnswers = qAnswers;
                                await _qsRepo.UpdateQuestionAsync(q);
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(a.Value)) continue;
                        
                        hasAnswersWithoutValue = true;
                        notAllAnswersEnteredErrors += $"на вопрос {n}, ";
                        break;
                    
                    }

                    if (qAnswers.Count < 2)
                    {
                        hasQuestionsWithNotEnoughAnswers = true;
                        notEnoughAnswersErrors += $"вопрос {n}, ";
                    }
                    
                    if (q.CorrectAnswerNumber != 0) continue;
                    hasUnchosenCorrectAnswers = true;
                    correctAnswerUnchosenErrors += $"вопроса {n}, ";
                }
                else if (q.AnswerType != AnswerType.HasToBeTyped)
                {
                    hasUndeclaredAnswerTypes = true;
                    answerTypeNotDeclaredErrors += $"вопроса {n}, ";
                }
            }

            static string ReplaceLastComma(string value) => Regex.Replace(value, @",\s$", ".");

            string msg;
            if (hasQuestionsWithoutValue)
            {
                msg = ReplaceLastComma(noQuestionValueErrors);
                errors.Add(msg);
            }
            if (hasHintsWithoutValue)
            {
                msg = ReplaceLastComma(noHintValueErrors);
                errors.Add(msg);
            }

            if (hasQuestionsWithoutAnswer)
            {     
                msg = ReplaceLastComma(noQuestionAnswerErrors);
                errors.Add(msg);
            }

            if (hasAnswersWithoutValue)
            {
                msg = ReplaceLastComma(notAllAnswersEnteredErrors);
                errors.Add(msg);
            }

            if (hasQuestionsWithNotEnoughAnswers)
            {
                msg = ReplaceLastComma(notEnoughAnswersErrors);
                errors.Add(msg);
            }

            if (hasUndeclaredAnswerTypes)
            {
                msg = ReplaceLastComma(answerTypeNotDeclaredErrors);
                errors.Add(msg);
            }

            if (hasUnchosenCorrectAnswers)
            {
                msg = ReplaceLastComma(correctAnswerUnchosenErrors);
                errors.Add(msg);
            }

            if (hasNullAnswers)
            {
                msg = ReplaceLastComma(nullAnswerErrors);
                errors.Add(msg);
            }

            if (errors.Count > 0) return _responseFactory.BadRequest(this, result: errors);


            test.IsPublished = true;
            await _testsRepo.UpdateTestAsync(test);

            return _responseFactory.Ok(this);
        }

        [HttpPost("/api/tests/rate-test")]
        public async Task<IActionResult> RateTest([FromForm] RateTestRequestModel model)
        {
            var rater = await _userManager.GetUserAsync(User);
            var test = await _testsRepo.GetTestAsync(model.TestId);
            if (test is null) return _responseFactory.NotFound(this);

            var rate = test.Rates.SingleOrDefault(r => r.User!.Equals(rater));

            if (rate is not null)
            {
                rate.Value = model.Rate;
                await _ratesRepo.UpdateRateAsync(rate);
            }
            else
            {
                rate = new Rate { Value = model.Rate, Test = test, User = rater };
                await _ratesRepo.AddRateAsync(rate);
            }
            return _responseFactory.Ok(this);
        }
    }
}