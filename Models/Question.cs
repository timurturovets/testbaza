namespace TestBaza.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        public int Number {get;set;}
        public string? Value { get; set; }
        public string? Answer { get; set; }

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
    }
}