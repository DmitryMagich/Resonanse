namespace Resonanse.Application.Dtos;

public class TrackDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int TrackNumber { get; set; }
    public int DiscNumber { get; set; }

    public Guid ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;

    public Guid AlbumId { get; set; }
    public string AlbumTitle { get; set; } = string.Empty;
}