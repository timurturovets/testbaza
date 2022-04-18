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
            .Include(t => t.Rates);

        public async Task<Test?> GetTestAsync(string testName)
        {
            return await _context.Tests
                .Where(t => t.TestName == testName)
                .Include(t => t.Questions).ThenInclude(q => q.MultipleAnswers)
                .Include(t => t.Creator)
                .Include(t => t.Rates).ThenInclude(r => r.User)
                .SingleOrDefaultAsync();
        }

        public async Task<Test?> GetTestAsync(int testId)
        {
            return await _context.Tests
                .Where(t => t.TestId == testId)
                .Include(t => t.Questions).ThenInclude(q => q.MultipleAnswers)
                .Include(t => t.Creator)
                .Include(t => t.Rates).ThenInclude(r => r.User)
                .Include(t=>t.PassingInfos).ThenInclude(i=>i.Attempts).ThenInclude(a=>a.UserAnswers)
                .SingleOrDefaultAsync();
        }

        public async Task<Test?> GetTestByLinkAsync(string link)
        {
            return await _context.Tests
                .Where(t => t.Link == link)
                .Include(t => t.Questions).ThenInclude(q => q.MultipleAnswers)
                .Include(t => t.Creator)
                .Include(t => t.Rates).ThenInclude(r => r.User)
                .SingleOrDefaultAsync();
        }
        public IEnumerable<Test> GetUserTests(User user)
        {
            return _context.Tests
                .Where(t => t.CreatorId == user.Id)
                .Include(t => t.Creator)
                .Include(t => t.Questions).ThenInclude(q => q.MultipleAnswers)
                .Include(t => t.Rates).ThenInclude(r => r.User);
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
