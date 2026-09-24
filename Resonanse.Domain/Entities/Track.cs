namespace Resonanse.Domain.Entities;

public class Track : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int TrackNumber { get; set; }
    public int DiscNumber { get; set; } = 1;

    public Guid AlbumId { get; set; }
    public Album Album { get; set; } = null!;

    public Guid ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;

    public ICollection<TrackFile> Files { get; set; } = new List<TrackFile>();
}