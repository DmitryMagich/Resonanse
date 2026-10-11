namespace Resonanse.Application.Scanning;

public class ScanRequest
{
    public Guid JobId { get; set; }
    public string Path { get; set; } = string.Empty;
}