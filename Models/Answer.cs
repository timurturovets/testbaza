namespace TestBaza.Models
{
    public class Answer
    {
        public int AnswerId { get; set; }

        public int Number { get; set; }
        public string? Value { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public AnswerJsonModel ToJsonModel()
        {
            return new AnswerJsonModel
            {
                Id = AnswerId,
                Number = Number,
                Value = Value
            };
        }
    }
}
