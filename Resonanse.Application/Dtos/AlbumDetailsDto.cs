namespace Resonanse.Application.Dtos;

public class AlbumDetailsDto : AlbumDto
{
    public List<TrackDto> Tracks { get; set; } = new();
}