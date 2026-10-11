using Resonanse.Domain.Enums;

namespace Resonanse.Domain.Entities;

/// <summary>
/// Represents a physical file of a track on a specific peer.
/// A single track may have multiple files across peers and formats.
/// </summary>
public class TrackFile : BaseEntity
{
    public Guid TrackId { get; set; }
    public Track Track { get; set; } = null!;

    public Guid PeerId { get; set; }
    public Peer Peer { get; set; } = null!;

    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// BLAKE3 hash (hex) of the file contents. Primary identifier in P2P network.
    /// </summary>
    public string FileHash { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public AudioFormat Format { get; set; } = AudioFormat.Unknown;
    public int Bitrate { get; set; }
    public int SampleRate { get; set; }
    public int? BitDepth { get; set; }
    public int Channels { get; set; }

    // P2P / external-source fields
    public TrackFileSource Source { get; set; } = TrackFileSource.Local;
    public string? ExternalId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsPinnedByUser { get; set; }
    public string? ExternalMetadataJson { get; set; }
}