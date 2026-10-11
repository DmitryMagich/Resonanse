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
                f.TrackId,
                f.FilePath,
                f.Format,
                f.FileSize,
                f.FileHash,
                f.Source,
                f.ExternalId
            })
            .FirstOrDefaultAsync(ct);

        if (file is null) return null;

        // Only local files are read from disk. External (Archive/YouTube) go through their own resolvers.
        if (file.Source == TrackFileSource.Local && !File.Exists(file.FilePath))
            return null;

        return new StreamInfoDto
        {
            TrackFileId = file.Id,
            TrackId = file.TrackId,
            FilePath = file.FilePath,
            ContentType = GetContentType(file.Format),
            FileSize = file.FileSize,
            FileHash = file.FileHash,
            Source = file.Source,
            ExternalId = file.ExternalId
        };
    }

    public async Task<StreamInfoDto?> GetBestStreamForTrackAsync(Guid trackId, string? preferredFormat, CancellationToken ct = default)
    {
        var files = await _db.TrackFiles
            .AsNoTracking()
            .Where(f => f.TrackId == trackId && f.Source == TrackFileSource.Local)
            .Select(f => new
            {
                f.Id,
                f.TrackId,
                f.FilePath,
                f.Format,
                f.FileSize,
                f.FileHash,
                f.Source,
                f.ExternalId
            })
            .ToListAsync(ct);

        if (files.Count == 0) return null;

        var existing = files.Where(f => File.Exists(f.FilePath)).ToList();
        if (existing.Count == 0) return null;

        if (!string.IsNullOrWhiteSpace(preferredFormat)
            && Enum.TryParse<AudioFormat>(preferredFormat, ignoreCase: true, out var wanted))
        {
            var match = existing.FirstOrDefault(f => f.Format == wanted);
            if (match is not null) return ToDto(match);
        }

        foreach (var fmt in TrackFormatHelper.Priority)
        {
            var match = existing.FirstOrDefault(f => f.Format == fmt);
            if (match is not null) return ToDto(match);
        }

        return ToDto(existing[0]);
    }

    public async Task<bool> IsTrackInTestAlbumAsync(Guid trackId, CancellationToken ct = default)
    {
        return await _db.Tracks
            .AsNoTracking()
            .Where(t => t.Id == trackId)
            .Select(t => t.Album.IsTestAlbum)
            .FirstOrDefaultAsync(ct);
    }

    private static StreamInfoDto ToDto(dynamic f) => new()
    {
        TrackFileId = f.Id,
        TrackId = f.TrackId,
        FilePath = f.FilePath,
        ContentType = GetContentType(f.Format),
        FileSize = f.FileSize,
        FileHash = f.FileHash,
        Source = f.Source,
        ExternalId = f.ExternalId
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