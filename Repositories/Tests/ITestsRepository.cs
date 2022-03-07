namespace TestBaza.Repositories
{
    public interface ITestsRepository
    {
        IEnumerable<Test> GetBrowsableTests();
        Test? GetTest(string testName);
        Test? GetTest(int testId);
        void AddTest(Test test);
        void RemoveTest(Test test);
        void UpdateTest(Test test);
    }
}
