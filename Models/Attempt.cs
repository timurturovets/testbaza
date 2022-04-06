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

        public AttemptJsonModel ToJsonModel()
        {
            return new AttemptJsonModel
            {
                TimeLeft = (int)(DateTime.Now - TimeStarted).TotalSeconds,
                UserAnswers = UserAnswers.Select(a => a.ToJsonModel()),
                Test = PassingInfo?.Test?.ToJsonModel(includeAnswers: false)
            };
        }
    }
}