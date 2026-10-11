using Resonanse.Domain.Enums;

namespace Resonanse.Application.Dtos;

public class ScanJobDto
{
    public Guid Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public ScanStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? Trigger { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double? DurationSeconds { get; set; }

    public int FilesProcessed { get; set; }
    public int TracksCreated { get; set; }
    public int TracksUpdated { get; set; }
    public int Errors { get; set; }

    public string? ErrorMessage { get; set; }
    public List<string> ErrorMessages { get; set; } = new();

    public Guid? StartedByUserId { get; set; }
    public string? StartedByUsername { get; set; }
}