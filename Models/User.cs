using Microsoft.AspNetCore.Identity;

namespace TestBaza.Models
{
    public class User : IdentityUser
    {
        public IEnumerable<Test> Tests { get; set; } = new List<Test>();
    }
}