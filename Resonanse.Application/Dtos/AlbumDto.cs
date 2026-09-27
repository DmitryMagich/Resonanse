namespace Resonanse.Application.Dtos;

public class AlbumDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? CoverPath { get; set; }

    public Guid ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;

    public int TrackCount { get; set; }
}