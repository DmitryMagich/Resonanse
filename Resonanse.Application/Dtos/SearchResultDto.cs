namespace Resonanse.Application.Dtos;

public class SearchResultDto
{
    public string Query { get; set; } = string.Empty;
    public List<TrackDto> Tracks { get; set; } = new();
    public List<AlbumDto> Albums { get; set; } = new();
    public List<ArtistDto> Artists { get; set; } = new();
}