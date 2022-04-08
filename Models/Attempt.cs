namespace TestBaza.Models
{
    public class Attempt
    {
        public int AttemptId { get; set; }

        public DateTime TimeStarted { get; set; }
        public DateTime TimeEnded { get; set; }
<<<<<<< HEAD
        public bool IsEnded { get; set; }
=======
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
        public IEnumerable<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        public int PassingInfoId { get; set; }
        public PassingInfo? PassingInfo { get; set; }

        public AttemptJsonModel ToJsonModel()
        {
            return new AttemptJsonModel
            {
<<<<<<< HEAD
                TimeLeft = PassingInfo!.Test!.TimeLimit - (int)(DateTime.Now - TimeStarted).TotalSeconds,
=======
                TimeLeft = (int)(DateTime.Now - TimeStarted).TotalSeconds,
>>>>>>> ca62367c9f4b2f3c7308104534a66a579eb725de
                UserAnswers = UserAnswers.Select(a => a.ToJsonModel()),
                Test = PassingInfo?.Test?.ToJsonModel(includeAnswers: false)
            };
        }
    }
}