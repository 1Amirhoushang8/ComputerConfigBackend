namespace Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string TrackingCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int? WorkerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string Status { get; set; } = TicketStatus.PendingPayment;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Customer? Customer { get; set; }
    public Worker? Worker { get; set; }
}

public static class TicketStatus
{
    public const string PendingPayment = "در انتظار پرداخت";
    public const string Paid = "پرداخت شده";
    public const string WaitingForPart = "در انتظار قطعه";
    public const string InRepair = "درحال تعمیر";
    public const string Repaired = "تعمیر شده";
    public const string Cancelled = "لغو شده";

    public static readonly string[] All = { PendingPayment, Paid, WaitingForPart, InRepair, Repaired, Cancelled };
}