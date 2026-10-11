using Resonanse.Domain.Enums;

namespace Resonanse.Application.Dtos;

public class InviteRequestDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string? Contact { get; set; }
    public InviteRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedByUserId { get; set; }
    public string? ProcessedByUsername { get; set; }
}