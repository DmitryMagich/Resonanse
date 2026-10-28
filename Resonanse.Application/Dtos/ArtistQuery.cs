using Resonanse.Domain.Enums;

namespace Resonanse.Application.Dtos;

public class ArtistQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public ArtistSortField SortBy { get; set; } = ArtistSortField.Name;
    public SortDirection SortDir { get; set; } = SortDirection.Asc;
    
    public ResonanceMode Mode { get; set; } = ResonanceMode.Offline;
}

public enum ArtistSortField
{
    Name = 0,
    AlbumCount = 1,
    TrackCount = 2,
    CreatedAt = 3
}