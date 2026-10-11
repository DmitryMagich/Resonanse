namespace Resonanse.Application.Services;

public interface ICoverService
{ 
    Task<string?> EnsureCoverForAlbumAsync(Guid albumId, string audioFilePath, CancellationToken ct = default);
}