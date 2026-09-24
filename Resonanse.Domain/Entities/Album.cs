namespace Resonanse.Domain.Entities;

public class Album : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? CoverPath { get; set; }
    public Guid ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}