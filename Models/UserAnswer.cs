namespace TestBaza.Models
{
    public class UserAnswer
    {
        public int UserAnswerId { get; set; }

        public string? Value { get; set; }
        public int QuestionNumber { get; set; }
        public bool IsCorrect
        {
            get
            {
                Test test = Attempt!.PassingInfo!.Test!;

                if (test.AreAnswersManuallyChecked) return false;

                Question question = test.Questions.Single(q => q.Number == QuestionNumber);

                string correctAnswer = question.AnswerType == AnswerType.HasToBeTyped
                    ? question.Answer!
                    : question.CorrectAnswerNumber + "";

                return correctAnswer == Value;
            }
            set { return; }
        }
        public int AttemptId { get; set; }
        public Attempt? Attempt { get; set; }

        public UserAnswerJsonModel ToJsonModel()
        {
            return new UserAnswerJsonModel
            {
                Value = Value,
                IsCorrect = IsCorrect,
                QuestionNumber = QuestionNumber
            };
        }
    }
}
