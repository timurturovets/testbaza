using TestBaza.Models.JsonModels;


namespace TestBaza.Models.RegularModels;

public class DetailedPassedTest
{
    public string? TestName { get; set; }
    public IEnumerable<UserAnswerJsonModel> UserAnswers { get; set; } = new List<UserAnswerJsonModel>();
    public IEnumerable<QuestionJsonModel> Questions { get; set; } = new List<QuestionJsonModel>();
    public int CorrectAnswersCount { get; set; }
    public bool AreAnswersManuallyChecked { get; set; }
    public bool IsChecked { get; set; } 
    public string? TimeUsed { get; set; }
}