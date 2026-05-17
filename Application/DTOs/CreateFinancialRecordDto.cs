namespace Application.DTOs;

public class CreateFinancialRecordDto
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public int? TicketId { get; set; }
    public string Description { get; set; } = string.Empty;
}