using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class VerifyOtpRequest
{
    [Required(ErrorMessage = "شماره موبایل الزامی است.")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل باید ۱۱ رقمی و با ۰۹ شروع شود.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "کد تأیید الزامی است.")]
    public string Code { get; set; } = string.Empty;
}