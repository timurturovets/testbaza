using System.ComponentModel.DataAnnotations;

namespace TestBaza.Models.DTOs;

public class LoginDto
{
    public string? Login { get; set; }

    [Required(ErrorMessage="Вы не ввели пароль")]
    public string? Password { get; set; }
}