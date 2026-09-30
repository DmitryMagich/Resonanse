namespace Resonanse.Domain.Entities;

public class Invite : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public Guid? UsedByUserId { get; set; }
    public User? UsedBy { get; set; }

    public DateTime? UsedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public bool IsActive => UsedAt is null && DateTime.UtcNow < ExpiresAt;
}