using Resonanse.Domain.Enums;

namespace Resonanse.Domain.Entities;

public class ScanJob : BaseEntity
{
    public string Path { get; set; } = string.Empty;
    public ScanStatus Status { get; set; } = ScanStatus.Pending;
    public string? Trigger { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public int FilesProcessed { get; set; }
    public int TracksCreated { get; set; }
    public int TracksUpdated { get; set; }
    public int Errors { get; set; }

    public string? ErrorMessage { get; set; }
    public string? ErrorMessagesJson { get; set; }

    public Guid? StartedByUserId { get; set; }
    public User? StartedBy { get; set; }
}