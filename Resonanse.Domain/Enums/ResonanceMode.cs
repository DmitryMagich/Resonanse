namespace Resonanse.Domain.Enums;

/// <summary>
/// Determines which content sources are included in API responses.
/// </summary>
public enum ResonanceMode
{
    /// <summary>Only locally stored files (this device's disk).</summary>
    Offline = 0,
    /// <summary>Local files plus external internet sources (Web Archive, yt-dlp).</summary>
    Online = 1,
    /// <summary>Everything: local, external, and P2P peers.</summary>
    Resonance = 2
}