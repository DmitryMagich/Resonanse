namespace Resonanse.Application.Dtos;

public class UpdateAlbumRequest
{
    public string? Title { get; set; }
    public int? ReleaseYear { get; set; }
    public string? CoverPath { get; set; }
    public Guid? ArtistId { get; set; }
}