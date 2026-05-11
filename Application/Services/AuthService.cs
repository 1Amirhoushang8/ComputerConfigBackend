using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class AuthService
{
    private readonly ComputerConfigContext _context;

    public AuthService(ComputerConfigContext context)
    {
        _context = context;
    }

    public async Task<(object? User, string Role)> AuthenticateAsync(string phoneNumber, string password)
    {
        // 1. Check Admins
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);
        if (admin != null && BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
            return (admin, "admin");

        // 2. Check Workers (only active)
        var worker = await _context.Workers
            .FirstOrDefaultAsync(w => w.PhoneNumber == phoneNumber && w.IsActive);
        if (worker != null && BCrypt.Net.BCrypt.Verify(password, worker.PasswordHash))
            return (worker, "worker");

        // 3. Check Customers
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        if (customer != null && BCrypt.Net.BCrypt.Verify(password, customer.PasswordHash))
            return (customer, "customer");

        return (null, string.Empty);
    }
}