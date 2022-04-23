using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;

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
            UserManager<User> manager = context.RequestServices.GetRequiredService<UserManager<User>>();
            User? user = await manager.GetUserAsync(context.User);

            if (user is null) await context.SignOutAsync();
            
            await _next(context);
        }
    }
}
