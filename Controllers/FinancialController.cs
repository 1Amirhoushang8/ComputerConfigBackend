using Application.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComputerConfigBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class FinancialController : ControllerBase
{
    private readonly ComputerConfigContext _context;

    public FinancialController(ComputerConfigContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<FinancialRecordListItemDto>>> GetAll()
    {
        var records = await _context.FinancialRecords
            .OrderByDescending(r => r.DateTime)
            .Select(r => new FinancialRecordListItemDto
            {
                Id = r.Id,
                Title = r.Title,
                Amount = r.Amount,
                DateTime = r.DateTime,
                TicketTrackingCode = r.Ticket != null ? r.Ticket.TrackingCode : null,
                Description = r.Description,
                Type = r.Type,
            })
            .ToListAsync();

        return Ok(records);
    }

    [HttpPost]
    public async Task<ActionResult<FinancialRecordListItemDto>> Create(CreateFinancialRecordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var record = new FinancialRecord
        {
            Title = dto.Title,
            Amount = dto.Amount,
            DateTime = dto.DateTime,
            TicketId = dto.TicketId,
            Description = dto.Description,
            Type = dto.Type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.FinancialRecords.Add(record);
        await _context.SaveChangesAsync();

        if (record.TicketId != null)
            await _context.Entry(record).Reference(r => r.Ticket).LoadAsync();

        return CreatedAtAction(nameof(GetAll), new { id = record.Id }, MapToDto(record));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FinancialRecordListItemDto>> Update(int id, UpdateFinancialRecordDto dto)
    {
        var record = await _context.FinancialRecords.FindAsync(id);
        if (record == null)
            return NotFound("گزارش مالی یافت نشد.");

        record.Title = dto.Title;
        record.Amount = dto.Amount;
        record.DateTime = dto.DateTime;
        record.TicketId = dto.TicketId;
        record.Description = dto.Description;
        record.Type = dto.Type;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (record.TicketId != null)
            await _context.Entry(record).Reference(r => r.Ticket).LoadAsync();

        return Ok(MapToDto(record));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await _context.FinancialRecords.FindAsync(id);
        if (record == null)
            return NotFound("گزارش مالی یافت نشد.");

        var deleted = new DeletedFinancialRecord
        {
            OriginalRecordId = record.Id,
            Title = record.Title,
            Amount = record.Amount,
            DateTime = record.DateTime,
            TicketId = record.TicketId,
            Description = record.Description,
            Type = record.Type,
            DeletedAt = DateTime.UtcNow
        };
        _context.DeletedFinancialRecords.Add(deleted);

        _context.FinancialRecords.Remove(record);
        await _context.SaveChangesAsync();

        return Ok(new { message = "گزارش مالی با موفقیت حذف شد." });
    }

    private FinancialRecordListItemDto MapToDto(FinancialRecord r)
    {
        return new FinancialRecordListItemDto
        {
            Id = r.Id,
            Title = r.Title,
            Amount = r.Amount,
            DateTime = r.DateTime,
            TicketTrackingCode = r.Ticket?.TrackingCode,
            Description = r.Description
        };
    }
}