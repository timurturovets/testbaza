namespace TestBaza.Repositories
{
    public interface IChecksInfoRepository
    {
        IEnumerable<CheckInfo> GetUserCheckInfos(User user);
    }
}