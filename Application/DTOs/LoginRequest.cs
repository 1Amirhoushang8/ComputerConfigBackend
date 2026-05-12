using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "شماره موبایل الزامی است.")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل باید ۱۱ رقمی و با ۰۹ شروع شود.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "رمز عبور فقط باید شامل حروف انگلیسی و اعداد باشد.")]
    public string Password { get; set; } = string.Empty;
}