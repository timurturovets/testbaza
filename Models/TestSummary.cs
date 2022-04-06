namespace TestBaza.Models
{
    /// <summary>
    /// Общая информация о тесте
    /// </summary>
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
        public int AllowedAttempts { get; set; }
        public bool AreAttemptsLimited { get; set; }
    }
}