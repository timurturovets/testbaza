using System.ComponentModel.DataAnnotations;

namespace TestBaza.Models
{
    public class UpdateTestRequestModel
    {
        public int TestId { get; set; }
        [Required]
        public string? TestName { get; set; }
        public string? Description { get; set; }
        public bool IsPrivate { get; set; }
        public bool AreAnswersManuallyChecked { get; set; }
        public TimeInfo TimeInfo { get; set; } = new(false, 0);
        public int AllowedAttempts { get; set; }
        public bool AreAttemptsLimited { get; set; }
    }
}