namespace TestBaza.Middlewares
{
    public class ClearSessionMiddleware
    {
        private readonly RequestDelegate _next;
        public ClearSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<ClearSessionMiddleware>>();
            string path = context.Request.Path;
            if (NON_API_ROUTES.Any(r=>path.StartsWith(r))) context.Session.SetString(API_KEY, string.Empty);
            await _next(context);
        }
    }
}