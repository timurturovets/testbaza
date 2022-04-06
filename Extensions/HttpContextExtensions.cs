using Microsoft.AspNetCore.Http;

namespace TestBaza.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetApiKey(this HttpContext context)
        {
            IConfiguration configuration = context.RequestServices.GetRequiredService<IConfiguration>();
            string value = configuration.GetValue<string>(API_KEY_NAME);
            return value;
        }
    }
}