namespace Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }           
    public string? UserRole { get; set; }
    public string? UserName { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}