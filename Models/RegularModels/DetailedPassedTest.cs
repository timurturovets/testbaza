namespace TestBaza.Models
{
    public class DetailedPassedTest
    {
        public string? TestName { get; set; }
        public IEnumerable<UserAnswerJsonModel> UserAnswers { get; set; } = new List<UserAnswerJsonModel>();
        public IEnumerable<QuestionJsonModel> Questions { get; set; } = new List<QuestionJsonModel>();
    }
}
