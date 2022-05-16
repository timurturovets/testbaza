using Microsoft.EntityFrameworkCore;

namespace TestBaza.Models
{
    public class UserAnswer
    {
        public int UserAnswerId { get; set; }

        public string? Value { get; set; }
        public int QuestionNumber { get; set; }
        private bool _isCorrect;

        [BackingField(nameof(_isCorrect))]
        public bool IsCorrect
        {
            get
            {
                var test = Attempt!.PassingInfo!.Test!;

                if (test.AreAnswersManuallyChecked) return _isCorrect;

                var question = test.Questions.FirstOrDefault(q => q.Number == QuestionNumber);

                var correctAnswer = question?.AnswerType == AnswerType.HasToBeTyped
                    ? question.Answer
                    : question?.CorrectAnswerNumber + "";

                return string.Equals(correctAnswer?.Trim(), Value?.Trim(), StringComparison.CurrentCultureIgnoreCase);
            }
            set 
            { 
                if(!Attempt?.PassingInfo?.Test?.AreAnswersManuallyChecked ?? false) return;
                _isCorrect = value;
            }
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