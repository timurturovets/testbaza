using Microsoft.EntityFrameworkCore;

using TestBaza.Data;

namespace TestBaza.Repositories
{
    public class PassingInfoRepository : IPassingInfoRepository
    {
        private readonly AppDbContext _context;
        public PassingInfoRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddInfoAsync(PassingInfo info)
        {
            await _context.PassingInfos.AddAsync(info);
            await _context.SaveChangesAsync();
        }

        public async Task<PassingInfo?> GetInfoAsync(User user, Test test)
        {
            return await _context.PassingInfos
                .Where(i => i.UserId == user.Id && i.TestId == test.TestId)
                .Include(i => i.Test).ThenInclude(t => t!.Creator)
                .Include(i => i.User).ThenInclude(u => u!.Tests)
                .Include(t => t.User!.Rates)
                .Include(i => i.Attempts).ThenInclude(a=>a.UserAnswers)
                .SingleOrDefaultAsync();
        }

        public async Task RemoveInfoAsync(PassingInfo info)
        {
            _context.PassingInfos.Remove(info);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateInfoAsync(PassingInfo info)
        {
            _context.PassingInfos.Update(info);
            await _context.SaveChangesAsync();
        }
    }
}
