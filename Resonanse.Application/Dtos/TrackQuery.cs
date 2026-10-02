namespace Resonanse.Application.Dtos;

public class TrackQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public TrackSortField SortBy { get; set; } = TrackSortField.Title;
    public SortDirection SortDir { get; set; } = SortDirection.Asc;

    // Фильтры
    public Guid? ArtistId { get; set; }
    public Guid? AlbumId { get; set; }
    public List<string>? Formats { get; set; }  // ["Flac", "Mp3"]
    public int? MinBitDepth { get; set; }        // 16, 24, 32
    public int? MinSampleRate { get; set; }      // 44100, 96000, 192000
    public bool? HasCover { get; set; }
}