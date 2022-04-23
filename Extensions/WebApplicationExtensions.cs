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
    }
}