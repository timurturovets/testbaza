namespace TestBaza.Models.DTOs;

public class SaveAnswerDto
{
    public int TestId { get; set; }
    public int QuestionNumber { get; set; }
    public string? Value { get; set; }
}