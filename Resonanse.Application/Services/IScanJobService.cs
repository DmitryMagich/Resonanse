using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public interface IScanJobService
{
    Task<ScanJobDto> EnqueueAsync(string? path, string trigger, Guid? userId, CancellationToken ct = default);
    Task<ScanJobDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ScanJobDto?> GetLatestAsync(CancellationToken ct = default);
    Task<bool> HasRunningJobAsync(CancellationToken ct = default);
    Task<List<ScanJobDto>> GetHistoryAsync(int limit, CancellationToken ct = default);
}