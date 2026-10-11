using Resonanse.Domain.Enums;

namespace Resonanse.Application.Dtos;

public class TrackFileDto
{
    public Guid Id { get; set; }
    public Guid PeerId { get; set; }
    public string PeerName { get; set; } = string.Empty;
    public AudioFormat Format { get; set; }
    public string FormatName => Format.ToString();
    public int Bitrate { get; set; }
    public int SampleRate { get; set; }
    public int? BitDepth { get; set; }
    public int Channels { get; set; }
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public TrackFileSource Source { get; set; }
    public string SourceName => Source.ToString();
    public string? ExternalId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsPinnedByUser { get; set; }
}