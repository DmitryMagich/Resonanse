namespace Resonanse.Application.Dtos.Auth;

public class InviteDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public Guid? UsedByUserId { get; set; }
    public string? UsedByUsername { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}