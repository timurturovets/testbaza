namespace TestBaza.Extensions;

public static class HttpContextExtensions
{
    public static T GetService<T>(this HttpContext context) where T: notnull
    {
        return context.RequestServices.GetRequiredService<T>();
    }

    /// <summary>
    /// Возвращает логгер, типизированный параметром типа Т
    /// </summary>
    /// <typeparam name="T">Параметр типа для логгера</typeparam>
    /// <returns>Логгер, типизированный Т</returns>
    public static ILogger<T> L<T>(this HttpContext context) where T : notnull
    {
        return context.GetService<ILogger<T>>();
    }
}