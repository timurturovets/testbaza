namespace TestsBaza.Repositories
{
    public interface ITestsRepository
    {
        IEnumerable<Test> GetAllTests();
        Test? GetTest(string testName);
        Test? GetTest(int testId);
        void AddTest(Test test);
        void RemoveTest(Test test);
        void UpdateTest(Test test);
    }
}
