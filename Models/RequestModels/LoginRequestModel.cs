using System.ComponentModel.DataAnnotations;

namespace TestBaza.Models
{
    public class LoginRequestModel
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }

        [Required(ErrorMessage="Вы не ввели пароль")]
        public string? Password { get; set; }
    }
}
