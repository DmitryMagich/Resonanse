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
            _logger.LogInformation("AutoScan: disabled by config.");
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
            await SafeScanAsync(path, "startup", stoppingToken);
        }
        
        if (_options.IntervalMinutes <= 0)
        {
            _logger.LogInformation("AutoScan: interval disabled, only startup scan runs.");
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
                await SafeScanAsync(path, "scheduled", stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task SafeScanAsync(string path, string reason, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var scanner = scope.ServiceProvider.GetRequiredService<ILibraryScanner>();

            _logger.LogInformation("AutoScan ({Reason}): starting for {Path}", reason, path);
            var startedAt = DateTime.UtcNow;

            var result = await scanner.ScanAsync(path, ct);

            _logger.LogInformation(
                "AutoScan ({Reason}): done in {Duration:F1}s — processed={Processed}, created={Created}, updated={Updated}, errors={Errors}",
                reason,
                (DateTime.UtcNow - startedAt).TotalSeconds,
                result.FilesProcessed,
                result.TracksCreated,
                result.TracksUpdated,
                result.Errors);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AutoScan ({Reason}): failed", reason);
        }
    }

    private string ResolvePath()
    {
        if (!string.IsNullOrWhiteSpace(_options.Path))
            return _options.Path!;

        var libPath = _configuration["Resonanse:LibraryPath"];
        if (!string.IsNullOrWhiteSpace(libPath))
            return libPath!;

        return DefaultPaths.GetMusicFolder();
    }
}