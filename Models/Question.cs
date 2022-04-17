namespace TestBaza.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        public int Number { get; set; }
        public string? Value { get; set; }
        public string? Hint { get; set; }
        public bool HintEnabled { get; set; }
        public string? Answer { get; set; }
        public IEnumerable<Answer> MultipleAnswers { get; set; } = new List<Answer>();
        public int CorrectAnswerNumber { get; set; }
        public AnswerType AnswerType { get; set; } = AnswerType.HasToBeTyped;

        public int TestId { get; set; }
        public Test? Test { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Question q)
                return q.QuestionId == QuestionId
                     && q.Value == Value
                     && q.Answer == Answer;
            return false;
        }
        public override int GetHashCode() => QuestionId.GetHashCode();

        public QuestionJsonModel ToJsonModel(bool includeAnswers = true)
        {
            return new QuestionJsonModel
            {
                QuestionId = QuestionId,
                Number = Number,
                Value = Value,
                Hint = Hint,
                HintEnabled = HintEnabled,
                Answer = includeAnswers ? Answer : string.Empty,
                Answers = MultipleAnswers.Select(a => a.ToJsonModel()),
                CorrectAnswerNumber = includeAnswers ? CorrectAnswerNumber : -1,
                AnswerType = (int)AnswerType,
            };
        }
    }
}