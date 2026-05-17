namespace Domain.Entities;

public class FinancialRecord
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }                        
    public DateTime DateTime { get; set; } = DateTime.UtcNow;  
    public int? TicketId { get; set; }                         
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    
    public Ticket? Ticket { get; set; }
}