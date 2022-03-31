namespace TestBaza.Repositories
{
    public interface IRatesRepository
    {
        Task AddRateAsync(Rate rate);
        Task UpdateRateAsync(Rate rate);
        Task DeleteRateAsync(Rate rate);
    }
}
