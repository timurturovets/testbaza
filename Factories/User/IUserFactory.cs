namespace TestBaza.Factories
{
    public interface IUserFactory
    {
        User Create(string userName, string email);
    }
}