namespace TestBaza.Models
{
    public class TestJsonModel
    {
        public int TestId { get; set; }
        public string? TestName { get; set; }
        public string? Description { get; set; }
        public IEnumerable<QuestionJsonModel> Questions { get; set; } = new List<QuestionJsonModel>();
        public string? AuthorName { get; set; }
        public TimeInfo TimeInfo { get; set; } = new(false, 0);
    }
}
