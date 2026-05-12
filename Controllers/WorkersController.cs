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
            .Where(w => w.IsActive)  
            .Select(w => new WorkerListItemDto
            {
                Id = w.Id,
                FullName = w.FullName,
                PhoneNumber = w.PhoneNumber,
                PersonalId = w.PersonalId,
                ActiveTicketCount = w.Tickets.Count(t => t.Status != TicketStatus.Delivered)
            })
            .ToListAsync();

        return Ok(workers);
    }
}