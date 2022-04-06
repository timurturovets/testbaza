using Microsoft.AspNetCore.Identity;

namespace TestBaza.Models
{
    public class User : IdentityUser
    {
        public IEnumerable<Test> Tests { get; set; } = new List<Test>();
        public IEnumerable<Rate> Rates { get; set; } = new List<Rate>();
        public IEnumerable<PassingInfo> PassingInfos { get; set; } = new List<PassingInfo>();

        public UserJsonModel ToJsonModel()
        {
            return new UserJsonModel
            {
                UserName = UserName,
                Email = Email,
                EmailConfirmed = EmailConfirmed
            };
        }
    }
}