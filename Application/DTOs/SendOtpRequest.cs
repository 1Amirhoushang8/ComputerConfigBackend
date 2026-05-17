using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class SendOtpRequest
{
    [Required(ErrorMessage = "شماره موبایل الزامی است.")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل باید ۱۱ رقمی و با ۰۹ شروع شود.")]
    public string PhoneNumber { get; set; } = string.Empty;
}