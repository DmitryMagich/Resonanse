namespace Resonanse.Api.Startup;

public class AutoScanOptions
{
    public const string SectionName = "Resonanse:AutoScan";
    public bool Enabled { get; set; } = true;
    public bool ScanOnStartup { get; set; } = true;
    /// Интервал между периодическими сканами в минутах. 0 = без периодичности.
    public int IntervalMinutes { get; set; } = 30;
    public string? Path { get; set; }
}