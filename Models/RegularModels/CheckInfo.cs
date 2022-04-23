namespace TestBaza.Models
{
    public class CheckInfo
    {
        public int CheckInfoId { get; set; }

        public bool IsChecked { get; set; }

        public string? CheckerId { get; set; }
        public User? Checker { get; set; }

        public int AttemptId { get; set; }
        public Attempt? Attempt { get; set; }

        public CheckInfoSummary ToSummary()
        {
            return new CheckInfoSummary
            {
                IsChecked = IsChecked,
                UserName = Attempt!.PassingInfo!.User!.UserName,
                AttemptId = AttemptId
            };
        }

        public CheckInfoJsonModel ToJsonModel()
        {
            return new CheckInfoJsonModel
            {
                TimeUsed = (Attempt!.TimeEnded - Attempt!.TimeStarted).ToString("HH:mm:ss"),
                UserAnswers = Attempt!.UserAnswers.Select(a => a.ToJsonModel()),
                Questions = Attempt!.PassingInfo!.Test!.Questions.Select(q => q.ToJsonModel())
            };
        }
    }
}