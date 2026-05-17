namespace Domain.Entities;

public class CustomerRequest
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int? TicketId { get; set; }                 
    public string Message { get; set; } = string.Empty; 
    public bool? Answer { get; set; }                   
    public DateTime? AnsweredAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    
    public Customer? Customer { get; set; }
    public Ticket? Ticket { get; set; }
}