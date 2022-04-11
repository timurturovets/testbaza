namespace TestBaza.Models
{
    public class PassingInfo
    {
        public int PassingInfoId { get; set; }

        public IEnumerable<Attempt> Attempts { get; set; } = new List<Attempt>();

        public string? UserId { get; set; }
        public User? User { get; set; }
        public int TestId { get; set; }
        public Test? Test { get; set; }

        public PassedTestSummary ToPassedTestSummary()
        {
            DateTime? time = Attempts.MaxBy(a => a.TimeEnded)?.TimeEnded;
            return new PassedTestSummary
            {
                TestId = TestId,
                TestName = Test!.TestName,
                AttemptsUsed = Attempts.Count(),
                LastTimePassed = $"{time?.ToShortDateString() ?? "--" } {time?.ToShortTimeString() ?? "--"}"
            };
        }
    }
}