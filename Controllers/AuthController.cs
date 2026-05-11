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
    private readonly AuthService _authService;
    private readonly TokenService _tokenService;
    private readonly ComputerConfigContext _context;

    public AuthController(AuthService authService, TokenService tokenService, ComputerConfigContext context)
    {
        _authService = authService;
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (user, role) = await _authService.AuthenticateAsync(request.PhoneNumber, request.Password);
        if (user == null)
            return Unauthorized("شماره تلفن یا رمز عبور اشتباه است.");

        string fullName = string.Empty;
        int userId = 0;
        string phone = request.PhoneNumber;

        switch (user)
        {
            case Admin admin:
                fullName = admin.FullName;
                userId = admin.Id;
                break;
            case Worker worker:
                fullName = worker.FullName;
                userId = worker.Id;
                break;
            case Customer customer:
                fullName = customer.FullName;
                userId = customer.Id;
                break;
        }

        // Generate the JWT and send it as an httpOnly cookie
        var token = _tokenService.GenerateToken(userId, fullName, role, phone);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,               
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddHours(8)
        };

        Response.Cookies.Append("auth_token", token, cookieOptions);

        // Return only the user info (no token in body)
        return Ok(new LoginResponse
        {
            FullName = fullName,
            Role = role
        });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        // Extract claims from the JWT (already validated by authentication middleware)
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
        // Clear the cookie
        Response.Cookies.Delete("auth_token");
        return Ok(new { message = "با موفقیت خارج شدید." });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
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

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        switch (request.Role)
        {
            case "admin":
                _context.Admins.Add(new Admin
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    PersonalId = request.PersonalId,
                    PasswordHash = passwordHash
                });
                break;
            case "worker":
                _context.Workers.Add(new Worker
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    PersonalId = request.PersonalId,
                    PasswordHash = passwordHash,
                    IsActive = true
                });
                break;
            default:
                _context.Customers.Add(new Customer
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    PersonalId = request.PersonalId,
                    PasswordHash = passwordHash
                });
                break;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "ثبت‌نام با موفقیت انجام شد." });
    }
}