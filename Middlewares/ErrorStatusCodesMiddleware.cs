namespace TestBaza.Middlewares
{
    public class ErrorStatusCodesMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorStatusCodesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
            switch (context.Response.StatusCode)
            {
                case 403:
                    context.Request.Path = "/home/forbidpage";
                    await _next(context);
                    break;
                case 404:
                    context.Request.Path = "/home/notfoundpage";
                    await _next(context);
                    break;
                default:
                    break;
            }
        }
    }
}
