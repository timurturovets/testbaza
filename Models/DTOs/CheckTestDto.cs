// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace TestBaza.Models.DTOs
{
    public class CheckTestDto
    {
        public int TestId { get; set; }
        public int AttemptId { get; set; }
        public List<int> CorrectAnswers { get; set; } = new();
        public List<int> IncorrectAnswers { get; set; } = new();
    }
}