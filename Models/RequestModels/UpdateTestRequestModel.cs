namespace TestBaza.Models
{
    public class UpdateTestRequestModel
    {     
        public int TestId { get; set; }
        public string? NewTestName { get; set; }
        public IEnumerable<QuestionJsonModel> NewQuestions { get; set; } = new List<QuestionJsonModel>();
    }
}