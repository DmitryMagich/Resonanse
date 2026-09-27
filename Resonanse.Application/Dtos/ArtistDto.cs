namespace Resonanse.Application.Dtos;

public class ArtistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public int AlbumCount { get; set; }
    public int TrackCount { get; set; }
}