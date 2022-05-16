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
            var userManager = context.RequestServices.GetRequiredService<UserManager<User>>();
            var signInManager = context.RequestServices.GetRequiredService<SignInManager<User>>();

            var user = await userManager.GetUserAsync(context.User);
            if (!(context.Request.Path + "").StartsWith("/auth"))
            {
                if (user is null) await signInManager.SignOutAsync();
            }
            await _next(context);
        }
    }
}