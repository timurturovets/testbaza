using TestBaza.Middlewares;

namespace TestBaza.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseErrorStatusCodesHandler(this WebApplication app)
        {
            app.UseMiddleware<ErrorStatusCodesMiddleware>();
            return app;
        }
    }
}
