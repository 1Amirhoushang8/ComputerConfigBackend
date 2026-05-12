using Application.DTOs;
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
}