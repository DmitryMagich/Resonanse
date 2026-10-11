using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public interface IErrorLogService
{
    Task<PagedResult<ErrorLogDto>> ListAsync(ErrorLogQuery query, CancellationToken ct = default);
    Task<ErrorLogDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<int> ClearAsync(DateTime? before, CancellationToken ct = default);
}