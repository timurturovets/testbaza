using Microsoft.AspNetCore.Identity;

using TestBaza.Models.JsonModels;


namespace TestBaza.Models.RegularModels;

public class User : IdentityUser
{
    public IEnumerable<Test> Tests { get; set; } = new List<Test>();
    public IEnumerable<Rate> Rates { get; set; } = new List<Rate>();
    public IEnumerable<PassingInfo> PassingInfos { get; set; } = new List<PassingInfo>();
    public IEnumerable<CheckInfo> CheckInfos { get; set; } = new List<CheckInfo>();
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