using TestBaza.Data;
using TestBaza.Factories;

namespace TestBaza.Middlewares;

public class DatabaseUpdatesMiddleware
{
    private readonly RequestDelegate _next;

    public DatabaseUpdatesMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string path = context.Request.Path;
        T GetService<T>() 
            => context.RequestServices.GetRequiredService<T>();

        if (path.StartsWith("/api/update-db"))
        {
            var dbTask = Task.Run(() =>
            {
                var content = GetService<AppDbContext>();
                var testFactory = GetService<ITestFactory>();
            });
            
            var requestTask = _next(context);
            Task.WaitAll(dbTask, requestTask);
        }
        else await _next(context);

    } 
}