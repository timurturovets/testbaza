namespace TestBaza.Models
{
    public class UserJsonModel
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public bool EmailConfirmed { get; set; }

        public IEnumerable<TestJsonModel> Tests { get; set; }
    }
}
