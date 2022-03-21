namespace TestBaza
{
    public static class Constants
    {
        public const string API_KEY = "API_KEY";
        public static readonly IList<string> NON_API_ROUTES = new List<string>(){ "/auth", "/home", "/profile" }.AsReadOnly();
        public const string DOMAIN_NAME = string.Empty; //TODO
    }
}