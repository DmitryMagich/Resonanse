namespace Resonanse.Application.Services;

public interface ILibraryScanner
{
    Task<ScanResult> ScanAsync(string rootPath, CancellationToken cancellationToken = default);
}

public class ScanResult
{
    public int FilesProcessed { get; set; }
    public int TracksCreated { get; set; }
    public int TracksUpdated { get; set; }
    public int Errors { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    public TimeSpan Duration { get; set; }
}
