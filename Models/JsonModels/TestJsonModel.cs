namespace TestBaza.Models
{
    public class TestJsonModel
    {
        public int Id { get; set; }
        public string? TestName { get; set; }
        public string? Description { get; set; }
        public bool IsPrivate { get; set; }
        public IEnumerable<QuestionJsonModel> Questions { get; set; } = new List<QuestionJsonModel>();
        public string? AuthorName { get; set; }
    }
}
