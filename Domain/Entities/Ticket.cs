namespace Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string TrackingCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int? WorkerId { get; set; }  // nullable until assigned
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string Status { get; set; } = TicketStatus.Received;  // default Persian status
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Customer? Customer { get; set; }
    public Worker? Worker { get; set; }
}

// Static class to hold Persian status constants
public static class TicketStatus
{
    public const string Received = "دریافت شد";
    public const string WaitingForPart = "در انتظار قطعه";
    public const string InRepair = "در حال تعمیر";
    public const string ReadyForDelivery = "آماده تحویل";
    public const string Delivered = "تحویل داده شد";
}