using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Resonanse.Infrastructure.Persistence;

namespace Resonanse.Infrastructure.Logging;

public class ErrorLogWriterService : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(2);

    private readonly ErrorLogChannel _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ErrorLogWriterService> _logger;

    public ErrorLogWriterService(
        ErrorLogChannel channel,
        IServiceScopeFactory scopeFactory,
        ILogger<ErrorLogWriterService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buffer = new List<Application.Dtos.ErrorLogDto>(BatchSize);
        var lastFlush = DateTime.UtcNow;

        try
        {
            await foreach (var entry in _channel.ReadAllAsync(stoppingToken))
            {
                buffer.Add(entry);

                var shouldFlush =
                    buffer.Count >= BatchSize ||
                    DateTime.UtcNow - lastFlush >= FlushInterval;

                if (shouldFlush)
                {
                    await FlushAsync(buffer, stoppingToken);
                    buffer.Clear();
                    lastFlush = DateTime.UtcNow;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }

        if (buffer.Count > 0)
        {
            try { await FlushAsync(buffer, CancellationToken.None); }
            catch { /* shutdown */ }
        }
    }

    private async Task FlushAsync(List<Application.Dtos.ErrorLogDto> entries, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ResonanseDbContext>();

            foreach (var e in entries)
            {
                db.ErrorLogs.Add(new Domain.Entities.ErrorLog
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
                });
            }

            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Ни в коем случае не логируем через DbLogger — иначе рекурсия
            _logger.LogWarning(ex, "Failed to persist {Count} error log entries to DB", entries.Count);
        }
    }
}