namespace Resonanse.Application.Services;

public interface IFavoriteService
{
    Task<bool> ToggleAsync(Guid trackId, CancellationToken ct = default);
    Task<bool> IsFavoriteAsync(Guid trackId, CancellationToken ct = default);
}