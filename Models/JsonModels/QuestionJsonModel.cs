namespace TestBaza.Models
{
    public class QuestionJsonModel
    {
        public int QuestionId { get; set; }
        public int Number { get; set; }
        public string? Value { get; set; }
        public string? Hint { get; set; }
        public bool HintEnabled { get; set; }
        public string? Answer { get; set; }
        public IEnumerable<AnswerJsonModel> Answers { get; set; } = new List<AnswerJsonModel>();
        public int AnswerType { get; set; }
    }
}