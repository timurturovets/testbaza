namespace TestBaza.Models
{
    public class Attempt
    {
        public int AttemptId { get; set; }

        public DateTime TimeStarted { get; set; }
        public DateTime TimeEnded { get; set; }
        public IEnumerable<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        public int PassingInfoId { get; set; }
        public PassingInfo? PassingInfo { get; set; }
    }
}
