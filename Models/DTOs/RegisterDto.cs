using System.ComponentModel.DataAnnotations;

namespace TestBaza.Models.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage ="Вы не ввели никнейм")]
    public string? UserName { get; set; }

    [Required(ErrorMessage ="Вы не ввели адрес эл. почты")]
    [EmailAddress(ErrorMessage ="Вы ввели некорректный адрес эл.почты")]
    public string? Email { get; set; }

    [Required(ErrorMessage ="Вы не ввели пароль")]
    public string? Password { get; set; }

    
    [Compare(nameof(Password), ErrorMessage ="Пароли не совпадают")]
    public string? ConfirmPassword { get; set; }
}