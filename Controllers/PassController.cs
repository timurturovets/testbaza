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
        private readonly IResponseFactory _responseFactory;
        public PassController(
            UserManager<User> userManager,
            ITestsRepository testsRepo,
            IResponseFactory responseFactory
            )
        {
            _userManager = userManager;
            _testsRepo = testsRepo;
            _responseFactory = responseFactory;
        }

        [HttpGet("/api/pass/info")]
        public IActionResult GetInfo([FromQuery] int id)
        {
            Test? test = _testsRepo.GetTest(id);
            if (test is null) return _responseFactory.NotFound(this);
            if (!test.IsBrowsable) return _responseFactory.Forbid(this);

            TestJsonModel model = test.ToJsonModel(includeAnswers: false);
            return _responseFactory.Ok(this, result: model);
        }
        [HttpPost("/api/pass/start-passing")]
        public async Task<IActionResult> StartPassing([FromForm] StartPassingRequestModel model)
        {
            Test? test = _testsRepo.GetTest(model.TestId);
            if (test is null) return _responseFactory.NotFound(this);

            User user = await  _userManager.GetUserAsync(User);
        }
    }
}