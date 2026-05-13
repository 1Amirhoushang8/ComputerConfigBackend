using Application.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComputerConfigBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,worker")]  
public class CustomersController : ControllerBase
{
    private readonly ComputerConfigContext _context;

    public CustomersController(ComputerConfigContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerListItemDto>>> GetCustomers()
    {
        var customers = await _context.Customers
            .OrderBy(c => c.Id)
            .Select(c => new CustomerListItemDto
            {
                Id = c.Id,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                PersonalId = c.PersonalId,
                TotalTickets = c.Tickets.Count
            })
            .ToListAsync();

        return Ok(customers);
    }


    // DELETE /api/customers/{id} – admin only
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound("مشتری یافت نشد.");

        // Archive the customer
        var deleted = new DeletedCustomer
        {
            OriginalCustomerId = customer.Id,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            PersonalId = customer.PersonalId,
            DeletedAt = DateTime.UtcNow
        };
        _context.DeletedCustomers.Add(deleted);

        // Remove the original
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return Ok(new { message = "مشتری با موفقیت حذف شد." });
    }
}
