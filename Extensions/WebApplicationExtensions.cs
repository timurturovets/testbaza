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

        public static IApplicationBuilder UseUserPresenceHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<AuthorizeMiddleware>();
            return app;
        }

        public static IApplicationBuilder UseDatabaseUpdateRequestsHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<DatabaseUpdatesMiddleware>();
            return app;
        }

        public static IEndpointRouteBuilder ToEndpointRouteBuilder(this IApplicationBuilder app)
        {
            return (IEndpointRouteBuilder)app;
        }
    }
}