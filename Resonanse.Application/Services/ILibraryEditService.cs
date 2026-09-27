using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public interface ILibraryEditService
{
    Task<TrackDto?> UpdateTrackAsync(Guid id, UpdateTrackRequest request, CancellationToken ct = default);
    Task<AlbumDto?> UpdateAlbumAsync(Guid id, UpdateAlbumRequest request, CancellationToken ct = default);
    Task<ArtistDto?> UpdateArtistAsync(Guid id, UpdateArtistRequest request, CancellationToken ct = default);
}   