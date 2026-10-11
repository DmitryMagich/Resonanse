using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Resonanse.Infrastructure.Persistence;

namespace Resonanse.Infrastructure.Logging;

public class ErrorLogCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ErrorLogCleanupService> _logger;
    private readonly int _retentionDays;

    public ErrorLogCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<ErrorLogCleanupService> logger,
        int retentionDays)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _retentionDays = retentionDays;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); }
        catch (OperationCanceledException) { return; }

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        do
        {
            await CleanupAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ResonanseDbContext>();

            var cutoff = DateTime.UtcNow.AddDays(-_retentionDays);
            var deleted = await db.ErrorLogs
                .Where(e => e.CreatedAt < cutoff)
                .ExecuteDeleteAsync(ct);

            if (deleted > 0)
                _logger.LogInformation("Cleaned up {Count} old error logs (older than {Days} days)", deleted, _retentionDays);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error log cleanup failed");
        }
    }
}