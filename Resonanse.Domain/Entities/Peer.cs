namespace Resonanse.Domain.Entities;

/// <summary>
/// Represents a node in the Resonance P2P network.
/// The local peer is this device; remote peers are friends' devices discovered via DHT.
/// </summary>
public class Peer : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// True for the local device's own peer. Only one peer should have this flag set.
    /// </summary>
    public bool IsLocal { get; set; }

    /// <summary>
    /// Ed25519 public key (base64). Identifies the peer in the P2P network.
    /// Populated by the Go agent.
    /// </summary>
    public string? PublicKey { get; set; }

    /// <summary>
    /// libp2p PeerId (base58). Populated by the Go agent.
    /// </summary>
    public string? PeerId { get; set; }

    /// <summary>
    /// True if this peer is the bootstrap node (has a public IP).
    /// Only the owner's server has this flag.
    /// </summary>
    public bool IsBootstrap { get; set; }

    public bool IsOnline { get; set; } = true;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    public ICollection<TrackFile> SharedFiles { get; set; } = new List<TrackFile>();
}