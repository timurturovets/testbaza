namespace TestBaza.Models
{
    public class TestJsonModel
    {
        public int TestId { get; set; }
        public string? TestName { get; set; }
        public string? Description { get; set; }
        public IEnumerable<QuestionJsonModel> Questions { get; set; } = new List<QuestionJsonModel>();
        public string? AuthorName { get; set; }
        public bool IsTimeLimited { get; set; }
        public int TimeLimit { get; set; }
    }
}
