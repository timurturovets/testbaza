namespace TestBaza.Models.JsonModels;

public class CheckInfoJsonModel
{
    public string? TimeUsed { get; set; }
    public IEnumerable<UserAnswerJsonModel> UserAnswers { get; set; } = new List<UserAnswerJsonModel>();
    public IEnumerable<QuestionJsonModel> Questions { get; set; } = new List<QuestionJsonModel>();
}