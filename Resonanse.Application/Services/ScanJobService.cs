using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;
using Resonanse.Application.Scanning;
using Resonanse.Domain.Entities;
using Resonanse.Domain.Enums;

namespace Resonanse.Application.Services;

public class ScanJobService : IScanJobService
{
    private readonly IResonanseDbContext _db;
    private readonly ScanQueue _queue;
    private readonly IConfiguration _configuration;

    public ScanJobService(
        IResonanseDbContext db,
        ScanQueue queue,
        IConfiguration configuration)
    {
        _db = db;
        _queue = queue;
        _configuration = configuration;
    }

    public async Task<ScanJobDto> EnqueueAsync(string? path, string trigger, Guid? userId, CancellationToken ct = default)
    {
        var scanPath = path;
        if (string.IsNullOrWhiteSpace(scanPath))
            scanPath = _configuration["Resonanse:LibraryPath"];
        if (string.IsNullOrWhiteSpace(scanPath))
            scanPath = _configuration["Resonanse:AutoScan:Path"];
        if (string.IsNullOrWhiteSpace(scanPath))
            scanPath = DefaultLibraryPath();

        if (!Directory.Exists(scanPath))
            throw new InvalidOperationException($"Directory not found: {scanPath}");

        var hasRunning = await _db.ScanJobs
            .AnyAsync(j => j.Status == ScanStatus.Running || j.Status == ScanStatus.Pending, ct);

        if (hasRunning)
            throw new InvalidOperationException("Another scan is already in progress.");

        var job = new ScanJob
        {
            Path = scanPath,
            Status = ScanStatus.Pending,
            Trigger = trigger,
            StartedByUserId = userId,
            StartedAt = DateTime.UtcNow
        };

        _db.ScanJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        var enqueued = _queue.TryEnqueue(new ScanRequest { JobId = job.Id, Path = job.Path });
        if (!enqueued)
        {
            job.Status = ScanStatus.Failed;
            job.ErrorMessage = "Scan queue is full.";
            job.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            throw new InvalidOperationException("Scan queue is full. Try again later.");
        }

        return Map(job, null);
    }

    public async Task<ScanJobDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _db.ScanJobs
            .AsNoTracking()
            .Include(j => j.StartedBy)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        return job is null ? null : Map(job, job.StartedBy?.Username);
    }

    public async Task<ScanJobDto?> GetLatestAsync(CancellationToken ct = default)
    {
        var job = await _db.ScanJobs
            .AsNoTracking()
            .Include(j => j.StartedBy)
            .OrderByDescending(j => j.StartedAt)
            .FirstOrDefaultAsync(ct);

        return job is null ? null : Map(job, job.StartedBy?.Username);
    }

    public async Task<bool> HasRunningJobAsync(CancellationToken ct = default)
    {
        return await _db.ScanJobs
            .AnyAsync(j => j.Status == ScanStatus.Running || j.Status == ScanStatus.Pending, ct);
    }

    public async Task<List<ScanJobDto>> GetHistoryAsync(int limit, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 100);

        var jobs = await _db.ScanJobs
            .AsNoTracking()
            .Include(j => j.StartedBy)
            .OrderByDescending(j => j.StartedAt)
            .Take(limit)
            .ToListAsync(ct);

        return jobs.Select(j => Map(j, j.StartedBy?.Username)).ToList();
    }

    private static ScanJobDto Map(ScanJob job, string? username)
    {
        List<string> errorMessages = new();
        if (!string.IsNullOrWhiteSpace(job.ErrorMessagesJson))
        {
            try
            {
                errorMessages = JsonSerializer.Deserialize<List<string>>(job.ErrorMessagesJson) ?? new();
            }
            catch
            {
                // ignore deserialization errors
            }
        }

        return new ScanJobDto
        {
            Id = job.Id,
            Path = job.Path,
            Status = job.Status,
            Trigger = job.Trigger,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            DurationSeconds = job.CompletedAt.HasValue
                ? (job.CompletedAt.Value - job.StartedAt).TotalSeconds
                : null,
            FilesProcessed = job.FilesProcessed,
            TracksCreated = job.TracksCreated,
            TracksUpdated = job.TracksUpdated,
            Errors = job.Errors,
            ErrorMessage = job.ErrorMessage,
            ErrorMessages = errorMessages,
            StartedByUserId = job.StartedByUserId,
            StartedByUsername = username
        };
    }

    private static string DefaultLibraryPath()
    {
        var myMusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        if (!string.IsNullOrEmpty(myMusic) && Directory.Exists(myMusic))
            return myMusic;

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, "Music");
    }
}