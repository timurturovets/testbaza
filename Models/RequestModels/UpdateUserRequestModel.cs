using System.ComponentModel.DataAnnotations;

namespace TestBaza.Models
{
    public class UpdateUserRequestModel
    {
        [Required(ErrorMessage = "Вы не ввели никнейм")]
        [RegularExpression(@"(?!\s)(?=([а-яА-ЯёЁ]|[a-zA-Z])).{4,20}",
            ErrorMessage = "Никнейм должен быть не короче 4 и не длиннее 20 символов, не содержать пробелов")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Вы не ввели адрес эл. почты")]
        [EmailAddress(ErrorMessage = "Вы ввели некорректный адрес эл.почты")]
        public string? Email { get; set; }
        public string? OldPassword { get; set; }

        [Required(ErrorMessage = "Вы не ввели пароль")]
        [RegularExpression(@"(?!\s)(?=.*([а-яА-ЯёЁ]|[a-zA-Z]))(?=.*[0-9]).{6,30}",
            ErrorMessage = "Пароль должен быть не короче 6 и не длиннее 30 символов, не содержать пробелов, содержать буквы и цифры")]
        public string? NewPassword { get; set; }
    }
}
