namespace Application.DTOs;

public class CustomerListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PersonalId { get; set; } = string.Empty;
    public int TotalTickets { get; set; }   
}