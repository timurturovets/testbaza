using TestBaza.Extensions;
using TestBaza.Models.Summaries;

namespace TestBaza.Models.RegularModels;

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
        var time = Attempts.OrderBy(a => a.TimeEnded).LastOrDefault()?.TimeEnded ?? default;
        return new PassedTestSummary
        {
            TestId = TestId,
            TestName = Test!.TestName,
            AttemptsUsed = Attempts.Count(),
            LastTimePassed = time.ToMskTimeString()
        };
    }
}