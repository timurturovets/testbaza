namespace TestBaza.Repositories
{
    public interface IRatesRepository
    {
        void AddRate(Rate rate);
        void UpdateRate(Rate rate);
        void DeleteRate(Rate rate);
    }
}
