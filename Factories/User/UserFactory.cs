namespace TestBaza.Factories
{
    public class UserFactory : IUserFactory
    {
        public User Create(string userName, string email)
        {
            return new User 
            { 
                UserName = userName,
                Email = email, 
                EmailConfirmed = false 
            };
        }
    }
}