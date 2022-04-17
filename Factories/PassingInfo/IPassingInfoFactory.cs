namespace TestBaza.Factories
{
    public interface IPassingInfoFactory
    {
        PassingInfo Create(User user, Test test);
    }
}