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
public class WorkersController : ControllerBase
{
    private readonly ComputerConfigContext _context;

    public WorkersController(ComputerConfigContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<WorkerListItemDto>>> GetWorkers()
    {
        var workers = await _context.Workers
            .Select(w => new WorkerListItemDto
            {
                Id = w.Id,
                FullName = w.FullName,
                PhoneNumber = w.PhoneNumber,
                PersonalId = w.PersonalId,
                Email = w.Email,
                Specialty = w.Specialty,
                // Active = not Repaired and not Cancelled
                ActiveTicketCount = w.Tickets.Count(t =>
                    t.Status != TicketStatus.Repaired &&
                    t.Status != TicketStatus.Cancelled)
            })
            .ToListAsync();

        return Ok(workers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkerListItemDto>> GetWorker(int id)
    {
        var worker = await _context.Workers
            .Where(w => w.Id == id)
            .Select(w => new WorkerListItemDto
            {
                Id = w.Id,
                FullName = w.FullName,
                PhoneNumber = w.PhoneNumber,
                PersonalId = w.PersonalId,
                Email = w.Email,
                Specialty = w.Specialty,
                // Active = not Repaired and not Cancelled
                ActiveTicketCount = w.Tickets.Count(t =>
                    t.Status != TicketStatus.Repaired &&
                    t.Status != TicketStatus.Cancelled)
            })
            .FirstOrDefaultAsync();

        if (worker == null)
            return NotFound("تعمیرکار یافت نشد.");

        return Ok(worker);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorker(int id, [FromBody] UpdateWorkerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var worker = await _context.Workers.FindAsync(id);
        if (worker == null)
            return NotFound("تعمیرکار یافت نشد.");

        // Uniqueness checks (only when actual values are provided)
        bool phoneConflict = await _context.Workers
            .AnyAsync(w => w.Id != id && w.PhoneNumber == dto.PhoneNumber);
        if (phoneConflict)
            return BadRequest("این شماره موبایل قبلاً ثبت شده است.");

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            bool emailConflict = await _context.Workers
                .AnyAsync(w => w.Id != id && w.Email == dto.Email);
            if (emailConflict)
                return BadRequest("این ایمیل قبلاً ثبت شده است.");
        }

        bool personalIdConflict = await _context.Workers
            .AnyAsync(w => w.Id != id && w.PersonalId == dto.PersonalId);
        if (personalIdConflict)
            return BadRequest("این کد ملی قبلاً ثبت شده است.");

        // Update fields
        worker.FullName = dto.FullName;
        worker.PhoneNumber = dto.PhoneNumber;
        worker.Email = dto.Email ?? string.Empty;
        worker.PersonalId = dto.PersonalId;
        worker.Specialty = dto.Specialty;

        await _context.SaveChangesAsync();
        return Ok(new { message = "اطلاعات تعمیرکار با موفقیت بروزرسانی شد." });
    }

    // DELETE /api/workers/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorker(int id)
    {
        var worker = await _context.Workers.FindAsync(id);
        if (worker == null)
            return NotFound("تعمیرکار یافت نشد.");

        // Archive the worker first
        var deleted = new DeletedWorker
        {
            OriginalWorkerId = worker.Id,
            FullName = worker.FullName,
            PhoneNumber = worker.PhoneNumber,
            Email = worker.Email,
            PersonalId = worker.PersonalId,
            Specialty = worker.Specialty,
            WasActive = worker.IsActive,
            DeletedAt = DateTime.UtcNow
        };
        _context.DeletedWorkers.Add(deleted);

        // Remove the original worker
        _context.Workers.Remove(worker);

        await _context.SaveChangesAsync();
        return Ok(new { message = "تعمیرکار با موفقیت حذف شد." });
    }
}