using System.ComponentModel.DataAnnotations;

namespace TestBaza.Models
{
    public class CreateTestRequestModel
    {
        [Required(ErrorMessage="Вы не ввели название теста")]
        [MinLength(4, ErrorMessage="Минимальная длина названия теста - 4 символа")]
        [MaxLength(35, ErrorMessage="Максимальная длина названия теста- 35 символов")]
        public string? TestName { get; set; }

        [MaxLength(100, ErrorMessage=$"Описание не должно содержать более 100 символов")]
        public string? Description { get; set; }
        public bool IsPrivate { get; set; } = false;
        public TimeInfo? TimeInfo { get; set; }
    }
}