namespace TestBaza.Models.Summaries;
public class PassedTestSummary
{
    public int TestId { get; set; }
    public string? TestName { get; set; }
    public int UserRate { get; set; }
    public string? LastTimePassed { get; set; }
    public int AttemptsUsed { get; set; }
}