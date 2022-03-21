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
            logger.LogCritical($"{path} starts with /api/tests/wq/: {path.StartsWith("/api/tests/wq/")}");
            if (path.StartsWith("/api/tests/wq/"))
            {
                logger.LogError($"Path in apikeysmw: {path}");

                ISession session = context.Session;

                string apiKey = context.GetApiKey();
                string? clientKey = session.GetString("API_KEY");

                logger.LogError($"Api key from appsettings.json: {apiKey}");
                logger.LogError($"Client api key: {clientKey}");

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