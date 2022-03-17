using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TestBaza.Extensions
{
    public static class ModelStateExtensions
    {
        public static IEnumerable<string> ToStringEnumerable(this ModelStateDictionary modelState)
        {
            return modelState.Select(entry => {
                var query = entry.Value?.Errors.Select(e => e.ErrorMessage);
                if (query is null || !query.Any()) return string.Empty;
                else return query.Aggregate((x, y) => x + ", " + y.ToLower()[0] + y[1..]);
            }).Select(value => value + ". ");
        }
    }
}