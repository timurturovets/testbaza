namespace TestBaza.Repositories
{
    public interface ITestsRepository
    {
        IEnumerable<Test> GetBrowsableTests();
        Task<Test?> GetTestAsync(string testName);
        Task<Test?> GetTestAsync(int testId);
        IEnumerable<Test> GetUserTests(User creator);
        Task AddTestAsync(Test test);
        Task RemoveTestAsync(Test test);
        Task UpdateTestAsync(Test test);
    }
}
