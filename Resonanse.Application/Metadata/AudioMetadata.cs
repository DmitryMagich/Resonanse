using Resonanse.Domain.Enums;

namespace Resonanse.Application.Metadata;

public class AudioMetadata
{
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string? AlbumArtist { get; set; }
    public string? Album { get; set; }
    public int? TrackNumber { get; set; }
    public int? DiscNumber { get; set; }
    public int? Year { get; set; }
    public TimeSpan Duration { get; set; }
    public int Bitrate { get; set; }
    public int SampleRate { get; set; }
    public int? BitDepth { get; set; }
    public int Channels { get; set; }
    public AudioFormat Format { get; set; }
}