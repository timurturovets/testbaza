using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using TestBaza.Factories;

namespace TestBaza.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private readonly IUserFactory _userFactory;
        private readonly IResponseFactory _responseFactory;
        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUserFactory userFactory,
            IResponseFactory responseFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;

            _userFactory = userFactory;
            _responseFactory = responseFactory;
        }

        [HttpGet]
        [ActionName("reg")]
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(HttpContext.User)) 
                return _responseFactory.RedirectToAction(this, actionName: "index", controllerName: "home", null);
            return _responseFactory.View(this, viewName:"register");
        }

        [HttpPost]
        [ActionName("reg")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequestModel model)
        {
            if (ModelState.IsValid)
            {
                if ((await _userManager.FindByNameAsync(model.UserName)) is not null)
                {
                    ModelState.AddModelError(string.Empty, "Этот никнейм уже занят. Попробуйте другой");
                }
                else
                {
                    User user = _userFactory.Create(model.UserName!, model.Email!);
                    IdentityResult createResult = await _userManager.CreateAsync(user, model.Password);

                    if (createResult.Succeeded)
                    {
                        var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);
                        if (signInResult.Succeeded) 
                            return _responseFactory.RedirectToAction(this, actionName: "index", controllerName: "home", null);
                    }
                    ModelState.AddModelError(string.Empty, "Произоша ошибка при попытке зарегистрироваться. Попробуйте снова");
                }
            }
            return _responseFactory.View(this, viewName: "register", model: model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(HttpContext.User))
                return _responseFactory.RedirectToAction(this, actionName: "index", controllerName: "home", null);
            return _responseFactory.View(this);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            if (ModelState.IsValid)
            {
                //В поле Login пользователь может ввести как свой никнейм, так и эл. почту
                //Соответственно, проверяем оба варианта
                User? user = await _userManager.FindByNameAsync(model.Login);
                if (user is null) user = await _userManager.FindByEmailAsync(model.Login);

                if(user is not null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);
                    if (result.Succeeded)
                    {
                        return _responseFactory.RedirectToAction(this, actionName: "index", controllerName: "home", null);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty,
                            "Произошла ошибка при попытке войти в аккаунт. Проверьте правильность введённых данных");
                    }
                }
                else ModelState.AddModelError(string.Empty, "Вы ввели неверный логин");
            }
            return _responseFactory.View(this, model: model);
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
           
            return _responseFactory.RedirectToAction(this, actionName: "login", controllerName: "auth", null);
        }
    }
}