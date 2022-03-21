using TestBaza.Middlewares;

namespace TestBaza.Extensions
{
    public static class WebApplicationExtensions
    {
        public static IApplicationBuilder UseErrorStatusCodesHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorStatusCodesMiddleware>();
            return app;
        }
        public static IApplicationBuilder UseApiKeysHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<ApiKeysMiddleware>();
            return app;
        }
        public static IApplicationBuilder ClearSession(this IApplicationBuilder app)
        {
            app.UseMiddleware<ClearSessionMiddleware>();
            return app;
        }
    }
}
