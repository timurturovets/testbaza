namespace TestBaza.Repositories.CheckInfos;

public interface IChecksInfoRepository
{
    IEnumerable<CheckInfo> GetUserCheckInfos(User user);
}