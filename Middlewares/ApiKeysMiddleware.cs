using TestBaza.Extensions;

namespace TestBaza.Middlewares
{
    public class ApiKeysMiddleware
    {
        private readonly RequestDelegate _next;
        public ApiKeysMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<ApiKeysMiddleware>>();
            string path = context.Request.Path;
            if (path.StartsWith("/api/tests/wq/"))
            {

                ISession session = context.Session;

                string apiKey = context.GetApiKey();
                string? clientKey = session.GetString("API_KEY");

                if (apiKey != clientKey)
                {
                    context.Response.StatusCode = 403;
                    return;
                }
            }
            await _next(context);
        }
    }
}