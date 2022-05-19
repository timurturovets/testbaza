using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

using TestBaza.Factories;

namespace TestBaza.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private readonly IUserFactory _userFactory;
        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUserFactory userFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;

            _userFactory = userFactory;
        }

        [HttpGet]
        [ActionName("reg")]
        public IActionResult Register()
        {
            return _signInManager.IsSignedIn(HttpContext.User) 
                ? RedirectToAction("Index", "Home") 
                : View("Register");
        }

        [HttpPost]
        [ActionName("reg")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequestModel model)
        {
            if (!ModelState.IsValid) return View("Register", model);
            if (await _userManager.FindByNameAsync(model.UserName) is not null)
            {
                ModelState.AddModelError(string.Empty,
                    "Этот никнейм уже занят. Попробуйте другой");
            }
            else
            {
                var user = _userFactory.Create(model.UserName!, model.Email!);
                var createResult = await _userManager.CreateAsync(user, model.Password);

                if (createResult.Succeeded)
                {
                    var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);
                    if (signInResult.Succeeded) 
                        return RedirectToAction("index","home");
                }
                ModelState.AddModelError(string.Empty, "Произоша ошибка при попытке зарегистрироваться. Попробуйте снова");
            }
            return View("Register", model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return _signInManager.IsSignedIn(HttpContext.User) 
                ? RedirectToAction("index", "home") 
                : View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            if (!ModelState.IsValid) return View(model);
            //В поле Login пользователь может ввести как свой никнейм, так и эл. почту
            //Соответственно, проверяем оба варианта
            var user = await _userManager.FindByNameAsync(model.Login) 
                       ?? await _userManager.FindByEmailAsync(model.Login);

            if(user is not null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("index", "home");
                }
                ModelState.AddModelError(string.Empty,
                    "Произошла ошибка при попытке войти в аккаунт. Проверьте правильность введённых данных");
            }
            else ModelState.AddModelError(string.Empty, "Вы ввели неверный логин");
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("login", "auth");
        }
    }
}