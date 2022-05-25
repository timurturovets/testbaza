using TestBaza.Extensions;
using TestBaza.Models.Summaries;
using TestBaza.Models.JsonModels;

namespace TestBaza.Models.RegularModels;

public class Attempt
{
    public int AttemptId { get; set; }

    public DateTime TimeStarted { get; set; }
    public DateTime TimeEnded { get; set; }
    public bool IsEnded { get; set; }

    public IEnumerable<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    public int CheckInfoId { get; set; }
    public CheckInfo? CheckInfo { get; set; }

    public int PassingInfoId { get; set; }
    public PassingInfo? PassingInfo { get; set; }

    public AttemptJsonModel ToJsonModel()
    {
        return new AttemptJsonModel
        {
            TimeLeft = PassingInfo!.Test!.TimeLimit - (int)(DateTime.Now - TimeStarted).TotalSeconds,
            UserAnswers = UserAnswers.Select(a => a.ToJsonModel()),
            Test = PassingInfo?.Test?.ToJsonModel(includeAnswers: false)
        };
    }

    public DetailedPassedTest ToDetailedTest()
    {
        var test = PassingInfo!.Test!;
        return new DetailedPassedTest
        {
            TestName = test.TestName,
            AreAnswersManuallyChecked = test.AreAnswersManuallyChecked,
            IsChecked = CheckInfo?.IsChecked ?? false,
            UserAnswers = UserAnswers.Select(a => a.ToJsonModel()),
            Questions = test.Questions.Select(q => q.ToJsonModel()),
            CorrectAnswersCount = UserAnswers.Count(a => a.IsCorrect),
            TimeUsed = (TimeEnded - TimeStarted).ToString(@"hh\:mm\:ss")
        };
    }
    public UserStatSummary ToStatSummary()
        => new (PassingInfo!.User!.UserName, 
            AttemptId, 
            TimeEnded.ToMskTimeString());
    
}