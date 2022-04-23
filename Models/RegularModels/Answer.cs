namespace TestBaza.Models
{
    public class Answer
    {
        public int AnswerId { get; set; }

        public int Number { get; set; }
        public string? Value { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public override int GetHashCode() => AnswerId.GetHashCode();
        public override bool Equals(object? obj)
        {
            if(obj is Answer a)
                return a.AnswerId == AnswerId
                    && a.Number == Number
                    && a.Value == Value;
            
            return false;
        }
        public AnswerJsonModel ToJsonModel()
        {
            return new AnswerJsonModel
            {
                AnswerId = AnswerId,
                Number = Number,
                Value = Value
            };
        }
    }
}