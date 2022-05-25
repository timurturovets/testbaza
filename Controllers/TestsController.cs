using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Factories;
using TestBaza.Models.DTOs;
using TestBaza.Repositories;
using TestBaza.Models.Summaries;

namespace TestBaza.Controllers;

[Authorize]
public class TestsController : Controller
{
    private readonly IRatesRepository _ratesRepo;
    private readonly ITestsRepository _testsRepo;
    private readonly IQuestionsRepository _qsRepo;

    private readonly UserManager<User> _userManager;

    private readonly ITestFactory _testFactory;
    public TestsController(
        IRatesRepository ratesRepo,
        ITestsRepository testsRepo,
        IQuestionsRepository qsRepo,

        UserManager<User> userManager,

        ITestFactory testFactory
        )
    {
        _ratesRepo = ratesRepo;
        _testsRepo = testsRepo;
        _qsRepo = qsRepo;

        _userManager = userManager;

        _testFactory = testFactory;
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
        if (list.Count == 0) return NoContent();

        var testsSummaries = list.Select(t => t.ToSummary());
        return Ok(new {result = testsSummaries});
    }

    [HttpGet]
    public async Task<IActionResult> Pass([FromQuery] int id)
    {
        var test = await _testsRepo.GetTestAsync(id);

        if (test is null) return NotFound();

        if (!(test.IsPublished && test.IsBrowsable)) return Forbid();

        ViewData["TestId"] = id;
        return View();
    }

    [Route("/tests/share")]
    public async Task<IActionResult> PassByLink([FromQuery] string test)
    {
        var passingTest = await _testsRepo.GetTestByLinkAsync(test);
        if (passingTest is null) return NotFound();

        if (!passingTest.IsPublished) return Forbid();

        ViewData["TestId"] = passingTest.TestId;
        return View("Pass");
    }

    [HttpGet("/api/tests/wq/get-test{id:int}")]
    public async Task<IActionResult> GetTest([FromRoute] int id)
    {
        var test = await _testsRepo.GetTestAsync(id);
        var creator = await _userManager.GetUserAsync(User);

        if (test is null) return NotFound();

        if (!test.Creator!.Equals(creator)) return Forbid();

        var model = test.ToJsonModel();
        return Ok(new { result = model });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateTestDto model)
    {
        if (!ModelState.IsValid) return View();
        var creator = await _userManager.GetUserAsync(User);
        if (await _testsRepo.GetTestAsync(model.TestName!) is not null)
        {
            ViewData["Error"] = "Тест с таким названием уже существует.";
            return View();
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

        return RedirectToAction("edit", "tests", new {id});

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
            return RedirectToAction("index", "home");
        }
        if (test.Creator is null)
        {
            ViewData["Error"] = $"Произошла неизвестная ошибка. Попробуйте перезайти на страницу редактирования";
            return RedirectToAction("index", "home");
        }

        if (!test.Creator.Equals(creator)) return Forbid();
        ViewData["TestId"] = id;

        return View();
    }

    [HttpPut("/api/tests/wq/update-test")]
    public async Task<IActionResult> UpdateTest([FromForm] UpdateTestDto model)
    {
        if (!ModelState.IsValid) return BadRequest();
        var test = await _testsRepo.GetTestAsync(model.TestId);
        if (test is null) return NotFound();

        var creator = await _userManager.GetUserAsync(User);
        if (!test.Creator!.Equals(creator)) return Forbid();

        test.Update(model);
        var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        test.UpdateImage(model.Image, env);
        await _testsRepo.UpdateTestAsync(test);
            
        var testModel = test.ToJsonModel();
        return Ok(new{result=testModel});
    }

    [HttpPost("/api/tests/delete-test")]
    public async Task<IActionResult> DeleteTest([FromForm] int testId)
    {
        var creator = await _userManager.GetUserAsync(User);

        var test = await _testsRepo.GetTestAsync(testId);
        if (test is null) return NotFound();

        if (!test.Creator!.Equals(creator)) return Forbid();

        await _testsRepo.RemoveTestAsync(test);

        return Ok();
    }

    [HttpPut("/api/tests/add-question")]
    public async Task<IActionResult> AddQuestion([FromForm] int testId)
    {
        var test = await _testsRepo.GetTestAsync(testId);
        if (test is null) return NotFound();

        var creator = await _userManager.GetUserAsync(User);
        if (!test.Creator!.Equals(creator)) return Forbid();

        var number = test.Questions.Count() + 1;

        Question newQuestion = new()
        {
            Test = test,
            Number = number
        };

        await _qsRepo.AddQuestionAsync(newQuestion);
        var createdQuestion = _qsRepo.GetQuestion(test, number);
        var questionId = createdQuestion!.QuestionId;

        return Ok(new {result = new {questionId, number}});
    }

    [HttpPut("/api/tests/update-question")]
    public async Task<IActionResult> UpdateQuestion([FromForm] UpdateQuestionDto dto)
    {
        var question = await _qsRepo.GetQuestionAsync(dto.QuestionId);
        if (question is null) return NotFound();

        var creator = await _userManager.GetUserAsync(User);
        if (!question.Test!.Creator!.Equals(creator)) return Forbid();

        question.Update(dto);
        var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        question.UpdateImage(dto.Image, env);
        
        await _qsRepo.UpdateQuestionAsync(question);

        return Ok();
    }
    [HttpPost("/api/tests/delete-question")]
    public async Task<IActionResult> DeleteQuestion([FromForm] int questionId)
    {
        var question = await _qsRepo.GetQuestionAsync(questionId);
        if (question is null) return NotFound();

        var creator = await _userManager.GetUserAsync(User);
        if (!question.Test!.Creator!.Equals(creator)) return Forbid();
        await _qsRepo.DeleteQuestionAsync(question);
        return Ok();
    }

    [HttpPost("/api/tests/add-answer")]
    public async Task<IActionResult> AddAnswer(int questionId)
    {
        var question = await _qsRepo.GetQuestionAsync(questionId);
        var creator = await _userManager.GetUserAsync(User);

        if (question is null) return NotFound();
        if (!question.Test!.Creator!.Equals(creator)) return Forbid();

        var (answerId, number) = await _qsRepo.AddAnswerToQuestionAsync(question);
        return Ok(new {result = new {answerId, number}});
    }

    [HttpPost("/api/tests/delete-answer")]
    public async Task<IActionResult> DeleteAnswer(int answerId, int questionId)
    {
        var question = await _qsRepo.GetQuestionAsync(questionId);
        if (question is null) return NotFound();

        var creator = await _userManager.GetUserAsync(User);
        if (!question.Test!.Creator!.Equals(creator)) return Forbid();

        var answer = question.MultipleAnswers.SingleOrDefault(a => a.AnswerId == answerId);
        if (answer is null) return BadRequest();

        await _qsRepo.RemoveAnswerFromQuestionAsync(question, answer);
        return Ok();
    }

    [HttpGet("/api/tests/publish-test{testId:int}")]
    public async Task<IActionResult> PublishTest([FromRoute] int testId)
    {
        var test = await _testsRepo.GetTestAsync(testId);
        List<string> errors = new();

        if (test is null)
        {
            errors.Add("Произошла непредвиденная ошибка. Перезагрузите страницу.");
            return BadRequest(new{result=errors});
        }

        var creator = await _userManager.GetUserAsync(User);
        if (!test.Creator!.Equals(creator)) return Forbid();

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
            var qsWithSameNumbers = test.Questions
                .Where(qn => qn.Number == q.Number);
            
            var withSameNumbers = qsWithSameNumbers as Question[] 
                                    ?? qsWithSameNumbers.ToArray();
            
            var qsCount = withSameNumbers.Length;
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

        if (errors.Count > 0) return BadRequest(new{result = errors});


        test.IsPublished = true;
        await _testsRepo.UpdateTestAsync(test);

        return Ok();
    }

    [HttpPost("/api/tests/rate-test")]
    public async Task<IActionResult> RateTest([FromForm] RateTestDto model)
    {
        var rater = await _userManager.GetUserAsync(User);
        var test = await _testsRepo.GetTestAsync(model.TestId);
        if (test is null) return NotFound();

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
        return Ok();
    }
}