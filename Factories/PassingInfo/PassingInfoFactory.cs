namespace TestBaza.Factories
{
    public class PassingInfoFactory : IPassingInfoFactory
    {
        public PassingInfo Create(User user, Test test)
        {
            return new PassingInfo
            {
                User = user,
                Test = test
            };
        }
    }
}