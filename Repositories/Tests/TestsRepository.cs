using Microsoft.EntityFrameworkCore;

using TestBaza.Data;

namespace TestBaza.Repositories
{
    public class TestsRepository : ITestsRepository
    {
        private readonly AppDbContext _context;
        public TestsRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Test> GetBrowsableTests() => _context.Tests
            .Where(t => t.IsBrowsable)
            .Include(t => t.Questions).ThenInclude(q => q.MultipleAnswers)
            .Include(t => t.Creator)
            .Include(t=>t.Rates);

        public Test? GetTest(string testName)
        {
            if (!_context.Tests.Any(t => t.TestName == testName)) return null;
            return _context.Tests
                .Where(t => t.TestName == testName)
                .Include(t => t.Questions).ThenInclude(q => q.MultipleAnswers)
                .Include(t => t.Creator)
                .Include(t=>t.Rates)
                .Single();
        }

        public Test? GetTest(int testId)
        {
            if (!_context.Tests.Any(t => t.TestId == testId)) return null;
            return _context.Tests
                .Where(t => t.TestId == testId)
                .Include(t => t.Questions).ThenInclude(q => q.MultipleAnswers)
                .Include(t => t.Creator)
                .Include(t => t.Rates)
                .Single();
        }

        public IEnumerable<Test> GetUserTests(User user)
        {
            return _context.Tests.Include(t => t.Creator)
                .Where(t => t.Creator!.Equals(user))
                .Include(t => t.Questions).ThenInclude(q => q.MultipleAnswers)
                .Include(t => t.Rates);
        }
        public async Task AddTestAsync(Test test)
        {
            if (_context.Tests.Any(t => t.Equals(test))) return;
            await _context.Tests.AddAsync(test);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTestAsync(Test test)
        {
            if (!_context.Tests.Any(t => t.Equals(test))) return;
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTestAsync(Test test)
        {
            _context.Tests.Update(test);
            await _context.SaveChangesAsync();
        }
    }
}