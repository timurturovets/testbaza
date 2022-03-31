namespace TestBaza.Repositories
{
    public interface ITestsRepository
    {
        IEnumerable<Test> GetBrowsableTests();
        Test? GetTest(string testName);
        Test? GetTest(int testId);
        IEnumerable<Test> GetUserTests(User creator);
        Task AddTestAsync(Test test);
        Task RemoveTestAsync(Test test);
        Task UpdateTestAsync(Test test);
    }
}
