namespace Domain.Entities;

public class DeletedFinancialRecord
{
    public int Id { get; set; }
    public int OriginalRecordId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public int? TicketId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
}