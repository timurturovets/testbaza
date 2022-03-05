namespace TestBaza.Models
{
    public class Test
    {
        public int TestId { get; set; }

        public string? TestName { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public IEnumerable<Question> Questions { get; set; } = new List<Question>();
        public DateTime TimeCreated { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsReady { get => Questions.Count() > 5; set { return; } }

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
                Id = TestId,
                TestName = TestName,
                IsPrivate = IsPrivate,
                Description = Description,
                AuthorName = Creator!.UserName,
                Questions = Questions.Select(q => q.ToJsonModel())
            };
        }
    }
}