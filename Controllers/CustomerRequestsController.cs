using Application.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ComputerConfigBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerRequestsController : ControllerBase
{
    private readonly ComputerConfigContext _context;

    public CustomerRequestsController(ComputerConfigContext context)
    {
        _context = context;
    }

    // List requests for a specific customer (or all if no customerId query)
    [HttpGet]
    [Authorize(Roles = "admin,worker")]
    public async Task<ActionResult<List<CustomerRequestListItemDto>>> GetAll([FromQuery] int? customerId)
    {
        IQueryable<CustomerRequest> query = _context.CustomerRequests
            .Include(cr => cr.Customer)
            .Include(cr => cr.Ticket);

        if (customerId.HasValue)
            query = query.Where(cr => cr.CustomerId == customerId.Value);

        var requests = await query
            .OrderByDescending(cr => cr.CreatedAt)
            .Select(cr => new CustomerRequestListItemDto
            {
                Id = cr.Id,
                CustomerId = cr.CustomerId,
                CustomerName = cr.Customer != null ? cr.Customer.FullName : null,
                TicketId = cr.TicketId,
                TicketTrackingCode = cr.Ticket != null ? cr.Ticket.TrackingCode : null,
                Message = cr.Message,
                Answer = cr.Answer,
                AnsweredAt = cr.AnsweredAt,
                CreatedBy = cr.CreatedBy,
                CreatedAt = cr.CreatedAt
            })
            .ToListAsync();

        return Ok(requests);
    }

    // Create
    [HttpPost]
    [Authorize(Roles = "admin,worker")]
    public async Task<ActionResult<CustomerRequestListItemDto>> Create(CreateCustomerRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
            return BadRequest("مشتری یافت نشد.");

        var request = new CustomerRequest
        {
            CustomerId = dto.CustomerId,
            TicketId = dto.TicketId,
            Message = dto.Message,
            CreatedBy = User.FindFirstValue(ClaimTypes.Role) ?? "unknown",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CustomerRequests.Add(request);
        await _context.SaveChangesAsync();

        // Reload navigation for response
        await _context.Entry(request).Reference(cr => cr.Customer).LoadAsync();
        if (request.TicketId != null)
            await _context.Entry(request).Reference(cr => cr.Ticket).LoadAsync();

        return CreatedAtAction(nameof(GetAll), new { id = request.Id }, MapToDto(request));
    }

    // Update (admin/worker only)
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,worker")]
    public async Task<ActionResult<CustomerRequestListItemDto>> Update(int id, UpdateCustomerRequestDto dto)
    {
        var request = await _context.CustomerRequests.FindAsync(id);
        if (request == null)
            return NotFound("درخواست یافت نشد.");

        request.CustomerId = dto.CustomerId;
        request.TicketId = dto.TicketId;
        request.Message = dto.Message;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _context.Entry(request).Reference(cr => cr.Customer).LoadAsync();
        if (request.TicketId != null)
            await _context.Entry(request).Reference(cr => cr.Ticket).LoadAsync();

        return Ok(MapToDto(request));
    }

    // Delete
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,worker")]
    public async Task<IActionResult> Delete(int id)
    {
        var request = await _context.CustomerRequests.FindAsync(id);
        if (request == null)
            return NotFound("درخواست یافت نشد.");

        _context.CustomerRequests.Remove(request);
        await _context.SaveChangesAsync();
        return Ok(new { message = "درخواست با موفقیت حذف شد." });
    }

    // Answer endpoint (customer only)
    [HttpPut("{id}/answer")]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> Answer(int id, AnswerCustomerRequestDto dto)
    {
        var request = await _context.CustomerRequests.FindAsync(id);
        if (request == null)
            return NotFound("درخواست یافت نشد.");

        // Only the owner customer can answer
        var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (customerIdClaim == null || request.CustomerId != int.Parse(customerIdClaim))
            return Forbid();

        request.Answer = dto.Answer;
        request.AnsweredAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "پاسخ شما ثبت شد." });
    }

    private CustomerRequestListItemDto MapToDto(CustomerRequest cr)
    {
        return new CustomerRequestListItemDto
        {
            Id = cr.Id,
            CustomerId = cr.CustomerId,
            CustomerName = cr.Customer?.FullName,
            TicketId = cr.TicketId,
            TicketTrackingCode = cr.Ticket?.TrackingCode,
            Message = cr.Message,
            Answer = cr.Answer,
            AnsweredAt = cr.AnsweredAt,
            CreatedBy = cr.CreatedBy,
            CreatedAt = cr.CreatedAt
        };
    }
}