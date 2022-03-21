namespace TestBaza.Models
{
    public class TestSummary
    {
        public int TestId { get; set; }
        public string? TestName { get; set; }
        public string? AuthorName { get; set; }
        public string? TimeCreated { get; set; }
        public bool IsPublished { get; set; }
        public TimeInfo? TimeInfo { get; set; }
        public bool IsBrowsable { get; set; }
        public int QuestionsCount { get; set; }
        public int RatesCount { get; set; }
        public double AverageRate { get; set; }
    }
}