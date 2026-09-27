using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public interface IStreamService
{
    Task<StreamInfoDto?> GetStreamInfoAsync(Guid trackFileId, CancellationToken ct = default);
    Task<StreamInfoDto?> GetBestStreamForTrackAsync(Guid trackId, string? preferredFormat, CancellationToken ct = default);
}