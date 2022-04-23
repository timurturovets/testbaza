namespace TestBaza.Repositories
{
    public interface IPassingInfoRepository
    {
        Task AddInfoAsync(PassingInfo info);
        Task<PassingInfo?> GetInfoAsync(User user, Test test);
        IEnumerable<PassingInfo> GetUserInfos(User user);
        Task UpdateInfoAsync(PassingInfo info);
        Task RemoveInfoAsync(PassingInfo info);

    }
}