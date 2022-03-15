namespace TestBaza.Repositories
{
    public interface ITestsRepository
    {
        IEnumerable<Test> GetBrowsableTests();
        Test? GetTest(string testName);
        Test? GetTest(int testId);
        IEnumerable<Test> GetUserTests(User creator);
        void AddTest(Test test);
        void RemoveTest(Test test);
        void UpdateTest(Test test);
    }
}
