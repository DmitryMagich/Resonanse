namespace Resonanse.Application.Dtos;

public class StreamInfoDto
{
    public Guid TrackFileId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
}