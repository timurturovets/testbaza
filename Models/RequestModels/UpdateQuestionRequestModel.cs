namespace TestBaza.Models
{
    public class UpdateQuestionRequestModel
    {
        public int QuestionId { get; set; }
        public string? Value { get; set; }
        public string? Hint { get; set; }
        public bool HintEnabled { get; set; }
        public string? Answer { get; set; }
        public AnswerJsonModel[]? Answers { get; set; }
        public int CorrectAnswerNumber { get; set; }
        public AnswerType AnswerType { get; set; }
    }
}