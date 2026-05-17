using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ComputerConfigBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly ComputerConfigContext _context;

    public AuthController(TokenService tokenService, ComputerConfigContext context)
    {
        _tokenService = tokenService;
        _context = context;
    }

    // Step 1: Request a login code (OTP for customers, permanent for staff)
    [HttpPost("send-otp")]
    [EnableRateLimiting("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string phone = request.PhoneNumber;

        // ---------- 3-minute cooldown ----------
        var recentOtp = await _context.OtpCodes
            .Where(o => o.PhoneNumber == phone && !o.IsUsed && o.CreatedAt > DateTime.UtcNow.AddMinutes(-3))
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (recentOtp != null)
        {
            var remainingSeconds = (int)(recentOtp.CreatedAt.AddMinutes(3) - DateTime.UtcNow).TotalSeconds;
            remainingSeconds = Math.Max(remainingSeconds, 0);
            return BadRequest(new
            {
                message = $"لطفاً {remainingSeconds} ثانیه دیگر صبر کنید.",
                remainingSeconds
            });
        }
        // ---------------------------------------

        // Check admins
        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.PhoneNumber == phone);
        if (admin != null)
        {
            string code = GenerateSecureCode();
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(code);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[OTP] Permanent code for admin {phone}: {code}");
            return Ok(new { message = "کد ورود برای شما ارسال شد.", requiresOtp = true });
        }

        // Check workers
        var worker = await _context.Workers.FirstOrDefaultAsync(w => w.PhoneNumber == phone && w.IsActive);
        if (worker != null)
        {
            string code = GenerateSecureCode();
            worker.PasswordHash = BCrypt.Net.BCrypt.HashPassword(code);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[OTP] Permanent code for worker {phone}: {code}");
            return Ok(new { message = "کد ورود برای شما ارسال شد.", requiresOtp = true });
        }

        // Check customers
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phone);
        if (customer != null)
        {
            // Invalidate any still-unused old codes for this phone
            var oldCodes = await _context.OtpCodes
                .Where(o => o.PhoneNumber == phone && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
            foreach (var old in oldCodes)
            {
                old.IsUsed = true;
            }

            string code = GenerateSecureCode();
            _context.OtpCodes.Add(new OtpCode
            {
                PhoneNumber = phone,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(3),   
                CreatedAt = DateTime.UtcNow,                  
                IsUsed = false
            });
            await _context.SaveChangesAsync();

            Console.WriteLine($"[OTP] One‑time code for customer {phone}: {code}");
            return Ok(new { message = "کد تأیید موقت برای شما ارسال شد.", requiresOtp = true });
        }

        return NotFound("کاربری با این شماره یافت نشد.");
    }

    // Step 2: Verify the code and log in
    [HttpPost("verify-otp")]
    [EnableRateLimiting("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string phone = request.PhoneNumber;
        string code = request.Code;

        // 1. Admin
        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.PhoneNumber == phone);
        if (admin != null && BCrypt.Net.BCrypt.Verify(code, admin.PasswordHash))
        {
            return await IssueToken(admin.Id, admin.FullName, "admin", phone);
        }

        // 2. Worker
        var worker = await _context.Workers.FirstOrDefaultAsync(w => w.PhoneNumber == phone && w.IsActive);
        if (worker != null && BCrypt.Net.BCrypt.Verify(code, worker.PasswordHash))
        {
            return await IssueToken(worker.Id, worker.FullName, "worker", phone);
        }

        // 3. Customer (one‑time code)
        var otp = await _context.OtpCodes
            .Where(o => o.PhoneNumber == phone && o.Code == code && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (otp != null)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phone);
            if (customer != null)
            {
                otp.IsUsed = true;
                await _context.SaveChangesAsync();

                return await IssueToken(customer.Id, customer.FullName, "customer", phone);
            }
        }

        return Unauthorized("کد نامعتبر یا منقضی شده است.");
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var fullName = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var phone = User.FindFirstValue("phone");

        return Ok(new
        {
            id = userId,
            fullName,
            role,
            phone
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        return Ok(new { message = "با موفقیت خارج شدید." });
    }

    // Registration endpoint (unchanged)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.Role == "worker" && string.IsNullOrWhiteSpace(request.Specialty))
            return BadRequest("تخصص الزامی است.");

        if (request.Role == "admin" || request.Role == "worker")
        {
            if (!User.IsInRole("admin"))
                return Unauthorized("فقط ادمین می‌تواند کاربر ادمین یا تعمیرکار ایجاد کند.");
        }

        bool phoneExists = await _context.Admins.AnyAsync(a => a.PhoneNumber == request.PhoneNumber)
                        || await _context.Workers.AnyAsync(w => w.PhoneNumber == request.PhoneNumber)
                        || await _context.Customers.AnyAsync(c => c.PhoneNumber == request.PhoneNumber);
        if (phoneExists)
            return BadRequest("این شماره تلفن قبلاً ثبت شده است.");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            bool emailExists = await _context.Admins.AnyAsync(a => a.Email == request.Email)
                            || await _context.Workers.AnyAsync(w => w.Email == request.Email)
                            || await _context.Customers.AnyAsync(c => c.Email == request.Email);
            if (emailExists)
                return BadRequest("این ایمیل قبلاً ثبت شده است.");
        }

        // If no password is provided, generate a random temporary one
        string password = string.IsNullOrWhiteSpace(request.Password)
            ? GenerateSecureCode()          // 6‑digit random code
            : request.Password;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        switch (request.Role)
        {
            case "admin":
                _context.Admins.Add(new Admin
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email ?? string.Empty,
                    PersonalId = request.PersonalId,
                    PasswordHash = passwordHash
                });
                break;
            case "worker":
                _context.Workers.Add(new Worker
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email ?? string.Empty,
                    PersonalId = request.PersonalId,
                    PasswordHash = passwordHash,
                    IsActive = true,
                    Specialty = request.Specialty ?? string.Empty
                });
                break;
            default:
                _context.Customers.Add(new Customer
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email ?? string.Empty,
                    PersonalId = request.PersonalId,
                    PasswordHash = passwordHash
                });
                break;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "ثبت‌نام با موفقیت انجام شد." });
    }

    // ---- Helpers ----
    private async Task<IActionResult> IssueToken(int userId, string fullName, string role, string phone)
    {
        var token = _tokenService.GenerateToken(userId, fullName, role, phone);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddHours(8)
        };
        Response.Cookies.Append("auth_token", token, cookieOptions);

        return Ok(new LoginResponse
        {
            FullName = fullName,
            Role = role
        });
    }

    private static string GenerateSecureCode()
    {
        // 6-digit random code
        return new Random().Next(100000, 999999).ToString();
    }
}