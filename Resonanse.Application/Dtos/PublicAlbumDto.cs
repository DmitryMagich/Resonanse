namespace Resonanse.Application.Dtos;

/// <summary>
/// Публичное представление тестового альбома — то, что видит анонимный гость.
/// </summary>
public class PublicAlbumDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? CoverPath { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public List<PublicTrackDto> Tracks { get; set; } = new();
}

public class PublicTrackDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int TrackNumber { get; set; }
    public string? Format { get; set; }  // лучший доступный
    public bool HasStream { get; set; }   // есть ли TrackFile локально
}