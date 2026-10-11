using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public class ErrorLogService : IErrorLogService
{
    private readonly IResonanseDbContext _db;

    public ErrorLogService(IResonanseDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ErrorLogDto>> ListAsync(ErrorLogQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var q = _db.ErrorLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Level))
            q = q.Where(e => e.Level == query.Level);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLowerInvariant();
            q = q.Where(e => e.Message.ToLower().Contains(s)
                             || (e.Path != null && e.Path.ToLower().Contains(s)));
        }

        if (query.From.HasValue)
            q = q.Where(e => e.CreatedAt >= query.From.Value);

        if (query.To.HasValue)
            q = q.Where(e => e.CreatedAt <= query.To.Value);

        if (query.UserId.HasValue)
            q = q.Where(e => e.UserId == query.UserId.Value);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ErrorLogDto
            {
                Id = e.Id,
                Level = e.Level,
                Message = e.Message,
                StackTrace = e.StackTrace,
                ExceptionType = e.ExceptionType,
                Path = e.Path,
                Method = e.Method,
                StatusCode = e.StatusCode,
                UserId = e.UserId,
                Username = e.Username,
                IpAddress = e.IpAddress,
                UserAgent = e.UserAgent,
                QueryString = e.QueryString,
                Category = e.Category,
                AdditionalData = e.AdditionalData,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<ErrorLogDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<ErrorLogDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.ErrorLogs
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new ErrorLogDto
            {
                Id = e.Id,
                Level = e.Level,
                Message = e.Message,
                StackTrace = e.StackTrace,
                ExceptionType = e.ExceptionType,
                Path = e.Path,
                Method = e.Method,
                StatusCode = e.StatusCode,
                UserId = e.UserId,
                Username = e.Username,
                IpAddress = e.IpAddress,
                UserAgent = e.UserAgent,
                QueryString = e.QueryString,
                Category = e.Category,
                AdditionalData = e.AdditionalData,
                CreatedAt = e.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> ClearAsync(DateTime? before, CancellationToken ct = default)
    {
        var cutoff = before ?? DateTime.UtcNow;

        return await _db.ErrorLogs
            .Where(e => e.CreatedAt < cutoff)
            .ExecuteDeleteAsync(ct);
    }
}