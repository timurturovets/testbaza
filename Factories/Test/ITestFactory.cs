namespace TestBaza.Factories
{
    public interface ITestFactory 
    {
        Test Create(string testName, string description, bool isPrivate, bool isTimeLimited, int timeLimit, User creator);
    }
}