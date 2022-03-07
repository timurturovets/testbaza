namespace TestBaza.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        public int Number {get;set;}
        public string? Value { get; set; }
        public string? Answer { get; set; }
        public IEnumerable<Answer> MultipleAnswers { get; set; } = new List<Answer>();
        public AnswerType AnswerType { get; set; } = AnswerType.HasToBeTyped;

        public int TestId { get; set; }
        public Test? Test { get; set; }


        public override bool Equals(object? obj)
        {
            if (obj is Question q)
                return q.QuestionId == QuestionId
                     && q.Value == Value
                     && q.Answer == Answer
                     && q.MultipleAnswers.SequenceEqual(MultipleAnswers);
            return false;
        }
        public override int GetHashCode() => QuestionId.GetHashCode();

        public QuestionJsonModel ToJsonModel()
        {
            return new QuestionJsonModel
            {
                QuestionId = QuestionId,
                Number = Number,
                Value = Value,
                AnswerType = (int)AnswerType,
                Answer = Answer,
                Answers = MultipleAnswers.Select(a => a.ToJsonModel())
            };
        }
    }

    public enum AnswerType
    {
        HasToBeTyped = 1,
        MultipleVariants = 2
    }
}