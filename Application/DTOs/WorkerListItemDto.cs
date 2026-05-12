namespace Application.DTOs;

public class WorkerListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PersonalId { get; set; } = string.Empty;
    public int ActiveTicketCount { get; set; }

   
    public string CurrentStatus => ActiveTicketCount > 0 ? "فعال" : "غیرفعال";
}