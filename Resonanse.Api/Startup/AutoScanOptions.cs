namespace Resonanse.Api.Startup;

public class AutoScanOptions
{
    public const string SectionName = "Resonanse:AutoScan";

    public bool Enabled { get; set; } = true;
    public bool ScanOnStartup { get; set; } = true;
    public int IntervalMinutes { get; set; } = 10;
    public string? Path { get; set; }
}