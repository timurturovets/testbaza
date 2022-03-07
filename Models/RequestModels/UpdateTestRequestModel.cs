namespace TestBaza.Models
{
    public class UpdateTestRequestModel
    {
        public int TestId { get; set; }
        public string? TestName { get; set; }
        public string? Description { get; set; }
        public bool IsPrivate { get; set; }
    }
}