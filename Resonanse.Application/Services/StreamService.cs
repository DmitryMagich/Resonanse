using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;
using Resonanse.Domain.Enums;

namespace Resonanse.Application.Services;

public class StreamService : IStreamService
{
    private readonly IResonanseDbContext _db;

    public StreamService(IResonanseDbContext db)
    {
        _db = db;
    }

    public async Task<StreamInfoDto?> GetStreamInfoAsync(Guid trackFileId, CancellationToken ct = default)
    {
        var file = await _db.TrackFiles
            .AsNoTracking()
            .Where(f => f.Id == trackFileId)
            .Select(f => new
            {
                f.Id,
                f.FilePath,
                f.Format,
                f.FileSize,
                f.FileHash
            })
            .FirstOrDefaultAsync(ct);

        if (file is null) return null;
        if (!File.Exists(file.FilePath)) return null;

        return new StreamInfoDto
        {
            TrackFileId = file.Id,
            FilePath = file.FilePath,
            ContentType = GetContentType(file.Format),
            FileSize = file.FileSize,
            FileHash = file.FileHash
        };
    }

    public async Task<StreamInfoDto?> GetBestStreamForTrackAsync(Guid trackId, string? preferredFormat, CancellationToken ct = default)
    {
        var files = await _db.TrackFiles
            .AsNoTracking()
            .Where(f => f.TrackId == trackId)
            .Select(f => new
            {
                f.Id,
                f.FilePath,
                f.Format,
                f.FileSize,
                f.FileHash
            })
            .ToListAsync(ct);

        if (files.Count == 0) return null;

        var existing = files.Where(f => File.Exists(f.FilePath)).ToList();
        if (existing.Count == 0) return null;

        // Если указан конкретный формат — берём его
        if (!string.IsNullOrWhiteSpace(preferredFormat)
            && Enum.TryParse<AudioFormat>(preferredFormat, ignoreCase: true, out var wantedFormat))
        {
            var match = existing.FirstOrDefault(f => f.Format == wantedFormat);
            if (match is not null)
                return ToDto(match.Id, match.FilePath, match.Format, match.FileSize, match.FileHash);
        }

        // Иначе — по приоритету качества
        var priority = new[]
        {
            AudioFormat.Flac,
            AudioFormat.Alac,
            AudioFormat.Wav,
            AudioFormat.Aiff,
            AudioFormat.Dsd,
            AudioFormat.Opus,
            AudioFormat.Ogg,
            AudioFormat.Aac,
            AudioFormat.Mp3,
            AudioFormat.Wma
        };

        foreach (var fmt in priority)
        {
            var match = existing.FirstOrDefault(f => f.Format == fmt);
            if (match is not null)
                return ToDto(match.Id, match.FilePath, match.Format, match.FileSize, match.FileHash);
        }

        var fallback = existing[0];
        return ToDto(fallback.Id, fallback.FilePath, fallback.Format, fallback.FileSize, fallback.FileHash);
    }

    private static StreamInfoDto ToDto(Guid id, string path, AudioFormat format, long size, string hash) => new()
    {
        TrackFileId = id,
        FilePath = path,
        ContentType = GetContentType(format),
        FileSize = size,
        FileHash = hash
    };

    private static string GetContentType(AudioFormat format) => format switch
    {
        AudioFormat.Flac => "audio/flac",
        AudioFormat.Alac => "audio/mp4",
        AudioFormat.Wav => "audio/wav",
        AudioFormat.Aiff => "audio/aiff",
        AudioFormat.Dsd => "audio/x-dsd",
        AudioFormat.Mp3 => "audio/mpeg",
        AudioFormat.Aac => "audio/aac",
        AudioFormat.Ogg => "audio/ogg",
        AudioFormat.Opus => "audio/opus",
        AudioFormat.Wma => "audio/x-ms-wma",
        _ => "application/octet-stream"
    };
}