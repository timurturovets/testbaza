namespace TestBaza.Models
{
    public class UserAnswer
    {
        public int UserAnswerId { get; set; }

        public string? Value { get; set; }

        public int QuestionId { get; set; }

        public Question? Question { get; set; }
    }
}
