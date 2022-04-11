namespace TestBaza.Models
{
    public class Rate
    {
        public int RateId { get; set; }

        public int Value { get; set; }
        
        public string? UserId { get; set; }
        public User? User { get; set; }

        public int TestId { get; set; }
        public Test? Test { get; set; }

    }
}
