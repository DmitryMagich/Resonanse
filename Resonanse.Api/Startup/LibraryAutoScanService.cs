using Microsoft.Extensions.Options;
using Resonanse.Application.Services;

namespace Resonanse.Api.Startup;

public class LibraryAutoScanService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AutoScanOptions _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LibraryAutoScanService> _logger;

    public LibraryAutoScanService(
        IServiceScopeFactory scopeFactory,
        IOptions<AutoScanOptions> options,
        IConfiguration configuration,
        ILogger<LibraryAutoScanService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("AutoScan: disabled by config");
            return;
        }

        var path = ResolvePath();
        if (!Directory.Exists(path))
        {
            _logger.LogWarning("AutoScan: path does not exist — {Path}. Idle.", path);
            return;
        }

        if (_options.ScanOnStartup)
        {
            await EnqueueAsync(path, "startup", stoppingToken);
        }

        if (_options.IntervalMinutes <= 0)
        {
            _logger.LogInformation("AutoScan: interval disabled, only startup scan runs");
            return;
        }

        var interval = TimeSpan.FromMinutes(_options.IntervalMinutes);
        _logger.LogInformation("AutoScan: scheduled every {Interval} min for {Path}",
            _options.IntervalMinutes, path);

        using var timer = new PeriodicTimer(interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await EnqueueAsync(path, "scheduled", stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
    }

    private async Task EnqueueAsync(string path, string trigger, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var scanJobs = scope.ServiceProvider.GetRequiredService<IScanJobService>();

            var job = await scanJobs.EnqueueAsync(path, trigger, userId: null, ct);
            _logger.LogInformation("AutoScan ({Trigger}): enqueued job {JobId} for {Path}",
                trigger, job.Id, path);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation("AutoScan ({Trigger}): skipped — {Reason}", trigger, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AutoScan ({Trigger}): failed to enqueue", trigger);
        }
    }

    private string ResolvePath()
    {
        if (!string.IsNullOrWhiteSpace(_options.Path))
            return _options.Path!;

        var libPath = _configuration["Resonanse:LibraryPath"];
        if (!string.IsNullOrWhiteSpace(libPath))
            return libPath!;

        var myMusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        if (!string.IsNullOrEmpty(myMusic) && Directory.Exists(myMusic))
            return myMusic;

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, "Music");
    }
}