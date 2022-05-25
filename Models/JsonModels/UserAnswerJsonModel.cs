namespace TestBaza.Models.JsonModels;

public class UserAnswerJsonModel
{
    public string? Value { get; set; }
    public int QuestionNumber { get; set; }
    public bool IsCorrect { get; set; }
}