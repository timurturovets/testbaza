namespace TestBaza.Models
{
    public class UserAnswer
    {
        public int UserAnswerId { get; set; }

        public string? Value { get; set; }
        public int QuestionNumber { get; set; }

        public int AttemptId { get; set; }
        public Attempt? Attempt { get; set; }

        public UserAnswerJsonModel ToJsonModel()
        {
            return new UserAnswerJsonModel
            {
                Value = Value,
                QuestionNumber = QuestionNumber
            };
        }
    }
}
