namespace TestBaza.Models.JsonModels;

public class AttemptJsonModel
{
    public int TimeLeft { get; set; }
    public IEnumerable<UserAnswerJsonModel> UserAnswers { get; set; } = new List<UserAnswerJsonModel>();
    public TestJsonModel? Test { get; set; }
}