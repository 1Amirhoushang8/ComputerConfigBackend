namespace Application.DTOs;

public class CreateCustomerRequestDto
{
    public int CustomerId { get; set; }
    public int? TicketId { get; set; }
    public string Message { get; set; } = string.Empty;
}