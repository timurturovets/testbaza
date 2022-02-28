using System.ComponentModel.DataAnnotations;

namespace TestBaza.Models
{
    public class LoginRequestModel
    {
        public string? Login { get; set; }

        [Required(ErrorMessage="Вы не ввели пароль")]
        public string? Password { get; set; }
    }
}