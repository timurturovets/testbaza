namespace TestBaza
{
    public static class Constants
    {
        public const string API_KEY_NAME = "API_KEY";
        public static IReadOnlyCollection<string> NON_API_ROUTES { get; } = new List<string>(){ "/auth", "/home", "/profile" }.AsReadOnly();
    }
}