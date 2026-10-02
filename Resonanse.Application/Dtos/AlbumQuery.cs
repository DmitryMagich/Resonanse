namespace Resonanse.Application.Dtos;

public class AlbumQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public AlbumSortField SortBy { get; set; } = AlbumSortField.Title;
    public SortDirection SortDir { get; set; } = SortDirection.Asc;

    public Guid? ArtistId { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public bool? HasCover { get; set; }
}

public enum AlbumSortField
{
    Title = 0,
    Artist = 1,
    ReleaseYear = 2,
    TrackCount = 3,
    CreatedAt = 4
}