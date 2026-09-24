namespace Resonanse.Domain.Entities;
/// <summary>
/// Узел сети.
/// </summary>
public class Peer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? PublicKey { get; set; }
    public bool IsOnline { get; set; } = true;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    public ICollection<TrackFile> SharedFiles { get; set; } = new List<TrackFile>();
}