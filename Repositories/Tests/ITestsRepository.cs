namespace TestBaza.Repositories
{
    public interface ITestsRepository
    {
        IEnumerable<Test> GetReadyTests();
        Test? GetTest(string testName);
        Test? GetTest(int testId);
        void AddTest(Test test);
        void RemoveTest(Test test);
        void UpdateTest(Test test);
    }
}
