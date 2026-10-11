using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Scanning;
using Resonanse.Application.Services;
using Resonanse.Domain.Enums;

namespace Resonanse.Infrastructure.Scanning;

public class ScanWorkerService : BackgroundService
{
    private readonly ScanQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScanWorkerService> _logger;

    public ScanWorkerService(
        ScanQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ScanWorkerService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessAsync(request, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scan worker failed for job {JobId}", request.JobId);
            }
        }
    }

    private async Task ProcessAsync(ScanRequest request, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IResonanseDbContext>();
        var scanner = scope.ServiceProvider.GetRequiredService<ILibraryScanner>();

        var job = await db.ScanJobs.FirstOrDefaultAsync(j => j.Id == request.JobId, ct);
        if (job is null)
        {
            _logger.LogWarning("Scan job {JobId} not found", request.JobId);
            return;
        }

        job.Status = ScanStatus.Running;
        job.StartedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        _logger.LogInformation("Scan job {JobId} started for {Path}", job.Id, job.Path);

        try
        {
            var result = await scanner.ScanAsync(job.Path, ct);

            job.Status = ScanStatus.Completed;
            job.FilesProcessed = result.FilesProcessed;
            job.TracksCreated = result.TracksCreated;
            job.TracksUpdated = result.TracksUpdated;
            job.Errors = result.Errors;
            job.ErrorMessagesJson = result.ErrorMessages.Count > 0
                ? JsonSerializer.Serialize(result.ErrorMessages)
                : null;
            job.CompletedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Scan job {JobId} completed: processed={Processed}, created={Created}, updated={Updated}, errors={Errors}",
                job.Id, result.FilesProcessed, result.TracksCreated, result.TracksUpdated, result.Errors);
        }
        catch (Exception ex)
        {
            job.Status = ScanStatus.Failed;
            job.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None);

            _logger.LogError(ex, "Scan job {JobId} failed", job.Id);
            throw;
        }
    }
}