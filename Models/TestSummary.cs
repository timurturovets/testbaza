namespace TestBaza.Models
{
    public class TestSummary
    {
        public string? TestName { get; set; }
        public string? AuthorName { get; set; }
        public DateTime TimeCreated { get; set; }
        public bool IsPublished { get; set; }
        public bool IsBrowsable { get; set; }
        public int QuestionsCount { get; set; }
        public double Rate { get; set; }
    }
}
