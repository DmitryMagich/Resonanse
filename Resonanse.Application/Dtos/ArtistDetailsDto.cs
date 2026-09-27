namespace Resonanse.Application.Dtos;

public class ArtistDetailsDto : ArtistDto
{
    public string? Bio { get; set; }
    public List<AlbumDto> Albums { get; set; } = new();
}