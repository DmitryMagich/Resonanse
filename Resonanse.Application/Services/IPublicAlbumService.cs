using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public interface IPublicAlbumService
{
    Task<PublicAlbumDto?> GetTestAlbumAsync(CancellationToken ct = default);
}