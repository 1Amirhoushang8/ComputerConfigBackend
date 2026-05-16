namespace Application.DTOs;

public class CreateTicketDto
{
    public string Title { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int WorkerId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
}