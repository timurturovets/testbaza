namespace TestBaza.Models
{
    public class CheckTestRequestModel
    {
        public int TestId { get; set; }
        public int AttemptId { get; set; }
        public List<int> CorrectAQNumbers { get; set; } = new();
        public List<int> IncorrectAQNumbers { get; set; } = new();
    }
}
