using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public interface ILibraryQueryService
{
    Task<PagedResult<TrackDto>> GetTracksAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<TrackDetailsDto?> GetTrackAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<AlbumDto>> GetAlbumsAsync(int page, int pageSize, Guid? artistId, CancellationToken ct = default);
    Task<AlbumDetailsDto?> GetAlbumAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ArtistDto>> GetArtistsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ArtistDetailsDto?> GetArtistAsync(Guid id, CancellationToken ct = default);
    Task<SearchResultDto> SearchAsync(string query, int limit, CancellationToken ct = default);
}