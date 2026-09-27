namespace Resonanse.Application.Dtos;

public class TrackDetailsDto : TrackDto
{
    public List<TrackFileDto> Files { get; set; } = new();
}