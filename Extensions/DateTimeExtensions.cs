namespace TestBaza.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToMskTimeString(this DateTime time)
        {
            return time.ToUniversalTime().AddHours(3).ToString("dd.MM.yyyy HH:mm:ss");
        }
    }
}
