using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Application.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "نام کامل الزامی است.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است.")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل باید ۱۱ رقمی و با ۰۹ شروع شود.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "ایمیل نامعتبر است.")]
    public string Email { get; set; } = string.Empty;


    [Required(ErrorMessage = "کد ملی الزامی است.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید دقیقاً ۱۰ رقم باشد.")]
    public string PersonalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "رمز عبور فقط باید شامل حروف انگلیسی و اعداد باشد.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "نقش کاربر الزامی است.")]
    public string Role { get; set; } = "customer";

    public string? Specialty { get; set; }
}