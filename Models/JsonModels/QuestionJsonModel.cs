namespace TestBaza.Models.JsonModels;
public class QuestionJsonModel
{
    public int QuestionId { get; set; }
    public int Number { get; set; }
    public string? Value { get; set; }
    public string? ImageRoute { get; set; }
    public bool HasImage { get; set; }
    public string? Hint { get; set; }
    public bool HintEnabled { get; set; }
    public string? Answer { get; set; }
    public IEnumerable<AnswerJsonModel> Answers { get; set; } = new List<AnswerJsonModel>();
    public int CorrectAnswerNumber { get; set; }
    public int AnswerType { get; set; }
}