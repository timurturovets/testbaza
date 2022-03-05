namespace TestBaza.Models
{
    public class UpdateQuestionRequestModel
    {
        public int Id { get; set; }
        public string? Value { get; set; }
        public string? Answer { get; set; }
        public IEnumerable<AnswerJsonModel>? Answers { get; set; }
        public AnswerType AnswerType { get; set; }
    }
}