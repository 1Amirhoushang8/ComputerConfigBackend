namespace Application.DTOs;

public class FinancialRecordListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public string? TicketTrackingCode { get; set; }   
    public string Description { get; set; } = string.Empty;
}