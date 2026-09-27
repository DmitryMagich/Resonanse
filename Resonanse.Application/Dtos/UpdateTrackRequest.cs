namespace Resonanse.Application.Dtos;

public class UpdateTrackRequest
{
    public string? Title { get; set; }
    public int? TrackNumber { get; set; }
    public int? DiscNumber { get; set; }
    public Guid? ArtistId { get; set; }
    public Guid? AlbumId { get; set; }
}