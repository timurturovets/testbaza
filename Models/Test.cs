namespace TestBaza.Models
{
    public class Test
    {
        public int TestId { get; set; }

        public string? TestName { get; set; }
        public string? Description { get; set; }
        public DateTime TimeCreated { get; set; }
        public bool IsPrivate { get; set; }
        public string? Link { get; set;}
        public bool IsTimeLimited { get; set; }
        public int TimeLimit { get; set; } // В секундах
        public bool IsPublished { get; set; }
        public bool IsBrowsable { get => IsPublished && !IsPrivate; set { return; } }

        public IEnumerable<Question> Questions { get; set; } = new List<Question>();
        public IEnumerable<Rate> Rates { get; set; } = new List<Rate>();
        public string? CreatorId { get; set; }
        public User? Creator { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Test test)
                return test.TestId == TestId
                     && test.TestName == TestName
                     && test.Questions.SequenceEqual(Questions);
            return false;
        }
        public override int GetHashCode() => TestId.GetHashCode();

        public TestJsonModel ToJsonModel()
        {
            return new TestJsonModel
            {
                TestId = TestId,
                TestName = TestName,
                Description = Description,
                AuthorName = Creator!.UserName,
                TimeInfo = new TimeInfo(IsTimeLimited, TimeLimit),
                Questions = Questions.Select(q => q.ToJsonModel())
            };
        }
        public TestSummary ToSummary()
        {
            return new TestSummary
            {
                TestId = TestId,
                TestName = TestName,
                AuthorName = Creator!.UserName,
                QuestionsCount = Questions.Count(),
                TimeCreated = TimeCreated.ToUniversalTime().AddHours(3).ToString("dd.MM.yyyy HH:mm:ss"),
                RatesCount = Rates.Count(),
                AverageRate = Rates.Any() ? Rates.Select(r => r.Value).Average() : 0,
                IsBrowsable = IsBrowsable,
                IsPublished = IsPublished,
                TimeInfo = new TimeInfo(IsTimeLimited, TimeLimit)
            };
        }
    }
}