using System.Security.Cryptography;

namespace TestBaza.Factories
{
    public class TestFactory : ITestFactory
    {
        public Test Create(string testName, string description, bool isPrivate, bool isTimeLimited, int timeLimit, User creator)
        {
            return new Test {
                TestName = testName,
                Description = description,
                IsPrivate = isPrivate,
                Creator = creator,
                TimeCreated = DateTime.Now,
                IsTimeLimited = isTimeLimited,
                TimeLimit = timeLimit,
                Link = "test" + (Guid.NewGuid() + "")[0..6]
            };
        }
    }
}