namespace TestBaza.Models
{
    public class SaveAnswerRequestModel
    {
        public int TestId { get; set; }
        public int QuestionNumber { get; set; }
        public string? Value { get; set; }
    }
}
