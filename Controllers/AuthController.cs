using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace TestBaza.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [ActionName("reg")]
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(HttpContext.User)) 
                return RedirectToAction(actionName: "index", controllerName: "home");
            return View(viewName:"register");
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
                    User user = new()
                    {
                        UserName = model.UserName,
                        Email = model.Email
                    };
                    IdentityResult createResult = await _userManager.CreateAsync(user, model.Password);

                    if (createResult.Succeeded)
                    {
                        var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);
                        if (signInResult.Succeeded) return RedirectToAction(actionName: "index", controllerName: "home");
                    }
                    ModelState.AddModelError(string.Empty, "Произоша ошибка при попытке зарегистрироваться. Попробуйте снова");
                }
            }
            return View(viewName: "register",model: model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(HttpContext.User))
                return RedirectToAction(actionName: "index", controllerName: "home");
            return View();
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
                        return RedirectToAction(actionName: "index", controllerName: "home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty,
                            "Произошла ошибка при попытке войти в аккаунт. Проверьте правильность введённых данных");
                    }
                }
                else ModelState.AddModelError(string.Empty, "Вы ввели неверный логин");
            }
            return View(model);
        }
    }
}
