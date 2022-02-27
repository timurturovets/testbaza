using Microsoft.EntityFrameworkCore;

using TestBaza.Data;

namespace TestsBaza.Repositories
{
    public class TestsRepository : ITestsRepository
    {
        private readonly AppDbContext _context;
        public TestsRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Test> GetAllTests() => _context.Tests
            .Include(t => t.Questions)
            .Include(t => t.Creator)
            .AsNoTracking();

        public Test? GetTest(string testName)
        {
            if (!_context.Tests.Any(t => t.TestName == testName)) return null;
            return _context.Tests.Where(t => t.TestName == testName)
                .Include(t => t.Questions)
                .Include(t => t.Creator)
                .Single();
        }
        public Test? GetTest(int testId)
        {
            if (!_context.Tests.Any(t => t.TestId == testId)) return null;
            return _context.Tests.Where(t => t.TestId == testId)
                .Include(t => t.Questions)
                .Include(t => t.Creator)
                .Single();
        }
        public void AddTest(Test test)
        {
            if (_context.Tests.Any(t => t.Equals(test))) return;
            _context.Tests.Add(test);
            _context.SaveChanges();
        }
        public void RemoveTest(Test test)
        {
            if (!_context.Tests.Any(t => t.Equals(test))) return;
            _context.Tests.Remove(test);
            _context.SaveChanges();
        }
        public void UpdateTest(Test test)
        {
            _context.Tests.Update(test);
            _context.SaveChanges();
        }
    }
}