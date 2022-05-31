using Microsoft.EntityFrameworkCore;

using TestBaza.Data;

namespace TestBaza.Repositories.CheckInfos;

public class ChecksInfoRepository : IChecksInfoRepository
{
    private readonly AppDbContext _context;
    public ChecksInfoRepository(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<CheckInfo> GetUserCheckInfos(User user)
    {
        return _context.CheckInfos
            .Include(a=>a.Attempt).ThenInclude(a=>a!.PassingInfo).ThenInclude(i=>i!.Test)
            .Where(i => i.Attempt != null
                && i.Attempt.PassingInfo != null
                && i.Attempt.PassingInfo.Test != null
                && i.Attempt.PassingInfo.Test.CreatorId == user.Id);
    }
}