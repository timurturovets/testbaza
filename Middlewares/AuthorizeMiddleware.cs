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
            UserManager<User> manager = context.RequestServices.GetRequiredService<UserManager<User>>();
            User? user = await manager.GetUserAsync(context.User);
            if (user is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            await _next(context);
        }
    }
}
