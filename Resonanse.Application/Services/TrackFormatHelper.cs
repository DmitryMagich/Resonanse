using Resonanse.Domain.Enums;

namespace Resonanse.Application.Services;

/// <summary>
/// Shared helpers for format-related presentation logic.
/// </summary>
public static class TrackFormatHelper
{
    public static readonly AudioFormat[] Priority =
    {
        AudioFormat.Flac,
        AudioFormat.Alac,
        AudioFormat.Mp3,
        AudioFormat.Aac,
        AudioFormat.Wav,
        AudioFormat.Aiff,
        AudioFormat.Dsd,
        AudioFormat.Opus,
        AudioFormat.Ogg,
        AudioFormat.Wma,
        AudioFormat.Unknown
    };
    
    public static AudioFormat? PickBest(IEnumerable<AudioFormat> formats)
    {
        var set = formats.ToHashSet();
        foreach (var f in Priority)
        {
            if (set.Contains(f)) return f;
        }
        return null;
    }
}