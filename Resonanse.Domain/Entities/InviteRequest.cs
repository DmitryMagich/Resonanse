using Resonanse.Domain.Enums;

namespace Resonanse.Domain.Entities;

/// <summary>
/// Заявка на получение инвайта от анонимного пользователя.
/// Админ рассматривает заявки и создаёт инвайт вручную.
/// </summary>
public class InviteRequest : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string? Contact { get; set; }

    public InviteRequestStatus Status { get; set; } = InviteRequestStatus.Pending;

    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedByUserId { get; set; }
    public User? ProcessedBy { get; set; }
}