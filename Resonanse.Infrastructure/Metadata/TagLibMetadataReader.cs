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

    public CoverMetadata? ReadCover(string filePath)
    {
        try
        {
            using var file = TagLib.File.Create(filePath);
            var pictures = file.Tag?.Pictures;
            if (pictures is null || pictures.Length == 0) return null;
            var cover = pictures.FirstOrDefault(p => p.Type == TagLib.PictureType.FrontCover)
                        ?? pictures[0];

            if (cover?.Data is null || cover.Data.Count == 0) return null;

            var bytes = cover.Data.Data;
            if (bytes.Length == 0) return null;

            var mime = string.IsNullOrWhiteSpace(cover.MimeType)
                ? GuessMimeFromBytes(bytes)
                : cover.MimeType;

            return new CoverMetadata(bytes, mime);
        }
        catch
        {
            return null;
        }
    }

    private static string GuessMimeFromBytes(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return "image/jpeg";

        if (bytes.Length >= 8
            && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return "image/png";

        if (bytes.Length >= 12
            && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46
            && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
            return "image/webp";

        return "image/jpeg";
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