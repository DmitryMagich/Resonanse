using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public interface ILibraryQueryService
{
    Task<PagedResult<TrackDto>> GetTracksAsync(TrackQuery query, CancellationToken ct = default);
    Task<TrackDetailsDto?> GetTrackAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<AlbumDto>> GetAlbumsAsync(AlbumQuery query, CancellationToken ct = default);
    Task<AlbumDetailsDto?> GetAlbumAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ArtistDto>> GetArtistsAsync(ArtistQuery query, CancellationToken ct = default);
    Task<ArtistDetailsDto?> GetArtistAsync(Guid id, CancellationToken ct = default);
    Task<SearchResultDto> SearchAsync(string query, int limit, CancellationToken ct = default);
}