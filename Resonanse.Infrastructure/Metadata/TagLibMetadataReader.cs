using Resonanse.Application.Abstractions;
using Resonanse.Application.Metadata;
using Resonanse.Domain.Enums;

namespace Resonanse.Infrastructure.Metadata;

public class TagLibMetadataReader : IMetadataReader
{
    public AudioMetadata? Read(string filePath)
    {
        using var file = TagLib.File.Create(filePath);
        if (file is null) return null;

        var tag = file.Tag;
        var props = file.Properties;

        return new AudioMetadata
        {
            Title = tag.Title ?? string.Empty,
            Artist = tag.FirstPerformer ?? tag.FirstAlbumArtist,
            AlbumArtist = tag.FirstAlbumArtist ?? tag.FirstPerformer,
            Album = tag.Album,
            TrackNumber = tag.Track > 0 ? (int)tag.Track : null,
            DiscNumber = tag.Disc > 0 ? (int)tag.Disc : null,
            Year = tag.Year > 0 ? (int)tag.Year : null,
            Duration = props?.Duration ?? TimeSpan.Zero,
            Bitrate = props?.AudioBitrate ?? 0,
            SampleRate = props?.AudioSampleRate ?? 0,
            BitDepth = props is { BitsPerSample: > 0 } ? props.BitsPerSample : null,
            Channels = props?.AudioChannels ?? 2,
            Format = DetectFormat(filePath)
        };
    }

    private static AudioFormat DetectFormat(string filePath) =>
        Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".flac" => AudioFormat.Flac,
            ".m4a" or ".alac" => AudioFormat.Alac,
            ".wav" => AudioFormat.Wav,
            ".aiff" or ".aif" => AudioFormat.Aiff,
            ".dsf" or ".dff" => AudioFormat.Dsd,
            ".mp3" => AudioFormat.Mp3,
            ".aac" => AudioFormat.Aac,
            ".ogg" => AudioFormat.Ogg,
            ".opus" => AudioFormat.Opus,
            ".wma" => AudioFormat.Wma,
            _ => AudioFormat.Unknown
        };
}