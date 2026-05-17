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
public class TicketsController : ControllerBase
{
    private readonly ComputerConfigContext _context;

    public TicketsController(ComputerConfigContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TicketListItemDto>>> GetTickets(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? customerId)

    {
        IQueryable<Ticket> query = _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.Worker);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.Trim().ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(s) ||
                (t.Customer != null ? t.Customer.FullName.ToLower().Contains(s) : false) ||
                t.TrackingCode.ToLower().Contains(s));
        }

        var tickets = await query
            .OrderByDescending(t => t.UpdatedAt)
            .Select(t => new TicketListItemDto
            {
                Id = t.Id,
                TrackingCode = t.TrackingCode,
                Title = t.Title,
                CustomerId = t.CustomerId,
                CustomerName = t.Customer != null ? t.Customer.FullName : "",
                WorkerId = t.WorkerId,
                WorkerName = t.Worker != null ? t.Worker.FullName : null,
                ServiceType = t.ServiceType,
                Status = t.Status,
                CreatedAt = DateTime.SpecifyKind(t.CreatedAt, DateTimeKind.Utc),
                UpdatedAt = DateTime.SpecifyKind(t.UpdatedAt, DateTimeKind.Utc),
                DeviceType = t.DeviceType,
                Brand = t.Brand,
                Model = t.Model,
                SerialNumber = t.SerialNumber,
                ProblemDescription = t.ProblemDescription
            })
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpPost]
    public async Task<ActionResult<TicketListItemDto>> CreateTicket([FromBody] CreateTicketDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("عنوان الزامی است.");
        if (dto.CustomerId <= 0)
            return BadRequest("مشتری الزامی است.");
        if (dto.WorkerId <= 0)
            return BadRequest("تعمیرکار الزامی است.");
        if (string.IsNullOrWhiteSpace(dto.ServiceType))
            return BadRequest("نوع سرویس الزامی است.");

        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
            return BadRequest("مشتری یافت نشد.");

        var worker = await _context.Workers.FindAsync(dto.WorkerId);
        if (worker == null)
            return BadRequest("تعمیرکار یافت نشد.");

        string trackingCode = GenerateTrackingCode();

        var ticket = new Ticket
        {
            TrackingCode = trackingCode,
            Title = dto.Title,
            CustomerId = dto.CustomerId,
            WorkerId = dto.WorkerId,
            ServiceType = dto.ServiceType,
            DeviceType = dto.DeviceType,
            Brand = dto.Brand,
            Model = dto.Model,
            SerialNumber = dto.SerialNumber,
            ProblemDescription = dto.ProblemDescription,
            Status = TicketStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        await _context.Entry(ticket).Reference(t => t.Customer).LoadAsync();
        await _context.Entry(ticket).Reference(t => t.Worker).LoadAsync();

        return CreatedAtAction(nameof(GetTickets), new { id = ticket.Id }, MapToDto(ticket));
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound("سرویس یافت نشد.");

        if (!TicketStatus.All.Contains(dto.Status))
            return BadRequest("وضعیت نامعتبر است.");

        ticket.Status = dto.Status;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        
        await _context.Entry(ticket).Reference(t => t.Customer).LoadAsync();
        if (ticket.WorkerId != null)
            await _context.Entry(ticket).Reference(t => t.Worker).LoadAsync();

        return Ok(MapToDto(ticket));  

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound("سرویس یافت نشد.");

        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("عنوان الزامی است.");
        if (dto.CustomerId <= 0)
            return BadRequest("مشتری الزامی است.");
        if (dto.WorkerId <= 0)
            return BadRequest("تعمیرکار الزامی است.");
        if (string.IsNullOrWhiteSpace(dto.ServiceType))
            return BadRequest("نوع سرویس الزامی است.");

        ticket.Title = dto.Title;
        ticket.CustomerId = dto.CustomerId;
        ticket.WorkerId = dto.WorkerId;
        ticket.ServiceType = dto.ServiceType;
        ticket.DeviceType = dto.DeviceType;
        ticket.Brand = dto.Brand;
        ticket.Model = dto.Model;
        ticket.SerialNumber = dto.SerialNumber;
        ticket.ProblemDescription = dto.ProblemDescription;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _context.Entry(ticket).Reference(t => t.Customer).LoadAsync();
        await _context.Entry(ticket).Reference(t => t.Worker).LoadAsync();

        return Ok(MapToDto(ticket));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound("سرویس یافت نشد.");

        var deleted = new DeletedTicket
        {
            OriginalTicketId = ticket.Id,
            TrackingCode = ticket.TrackingCode,
            Title = ticket.Title,
            CustomerId = ticket.CustomerId,
            WorkerId = ticket.WorkerId,
            ServiceType = ticket.ServiceType,
            DeviceType = ticket.DeviceType,
            Brand = ticket.Brand,
            Model = ticket.Model,
            SerialNumber = ticket.SerialNumber,
            ProblemDescription = ticket.ProblemDescription,
            Status = ticket.Status,
            DeletedAt = DateTime.UtcNow
        };
        _context.DeletedTickets.Add(deleted);

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();

        return Ok(new { message = "سرویس با موفقیت حذف شد." });
    }

    private string GenerateTrackingCode()
    {
        return "TRK-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" +
               Guid.NewGuid().ToString("N")[..6].ToUpper();
    }

    private static TicketListItemDto MapToDto(Ticket t)
    {
        return new TicketListItemDto
        {
            Id = t.Id,
            TrackingCode = t.TrackingCode,
            Title = t.Title,
            CustomerId = t.CustomerId,
            CustomerName = t.Customer?.FullName ?? "",
            WorkerId = t.WorkerId,
            WorkerName = t.Worker?.FullName,
            ServiceType = t.ServiceType,
            Status = t.Status,
            CreatedAt = DateTime.SpecifyKind(t.CreatedAt, DateTimeKind.Utc),
            UpdatedAt = DateTime.SpecifyKind(t.UpdatedAt, DateTimeKind.Utc),
            DeviceType = t.DeviceType,
            Brand = t.Brand,
            Model = t.Model,
            SerialNumber = t.SerialNumber,
            ProblemDescription = t.ProblemDescription
        };
    }
}