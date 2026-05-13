namespace Domain.Entities;

public class DeletedWorker
{
    public int Id { get; set; }
    public int OriginalWorkerId { get; set; }     
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PersonalId { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public bool WasActive { get; set; }           
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
}