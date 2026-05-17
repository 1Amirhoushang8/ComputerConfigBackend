namespace Application.DTOs;

public class CustomerRequestListItemDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? TicketId { get; set; }
    public string? TicketTrackingCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool? Answer { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}