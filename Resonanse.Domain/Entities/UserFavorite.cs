namespace Resonanse.Domain.Entities;

public class UserFavorite : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid TrackId { get; set; }
    public Track Track { get; set; } = null!;
}