using Microsoft.AspNetCore.Identity;

namespace TestBaza.Middlewares
{
    public class AuthorizeMiddleware
    {
        private readonly RequestDelegate _next;
        public AuthorizeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            UserManager<User> userManager = context.RequestServices.GetRequiredService<UserManager<User>>();
            SignInManager<User> signInManager = context.RequestServices.GetRequiredService<SignInManager<User>>();

            User? user = await userManager.GetUserAsync(context.User);
            if (!(context.Request.Path + "").StartsWith("/auth"))
            {
                if (user is null) await signInManager.SignOutAsync();
            }
            await _next(context);
        }
    }
}