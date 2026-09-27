using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Resonanse.Application.Abstractions;
using Resonanse.Domain.Entities;

namespace Resonanse.Application.Services;

public class LibraryScanner : ILibraryScanner
{
    private static readonly string[] SupportedExtensions =
    {
        ".mp3", ".flac", ".m4a", ".aac", ".ogg", ".opus",
        ".wav", ".aiff", ".aif", ".wma", ".dsf", ".dff"
    };

    private readonly IResonanseDbContext _db;
    private readonly IMetadataReader _metadataReader;
    private readonly IFileHasher _hasher;
    private readonly HashSet<string> _excludedFolders;

    public LibraryScanner(
        IResonanseDbContext db,
        IMetadataReader metadataReader,
        IFileHasher hasher,
        IConfiguration configuration)
    {
        _db = db;
        _metadataReader = metadataReader;
        _hasher = hasher;
        _excludedFolders = configuration
            .GetSection("Resonanse:ExcludedFolders")
            .Get<string[]>()
            ?.ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<ScanResult> ScanAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        var result = new ScanResult();

        if (!Directory.Exists(rootPath))
        {
            result.ErrorMessages.Add($"Directory not found: {rootPath}");
            return result;
        }

        var peer = await _db.Peers.FirstOrDefaultAsync(p => p.IsOnline, cancellationToken);
        if (peer is null)
        {
            result.ErrorMessages.Add("No local peer configured.");
            return result;
        }

        var files = EnumerateFilesSkippingExcluded(rootPath).ToList();

        foreach (var filePath in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await ProcessFileAsync(filePath, peer, result, cancellationToken);
                result.FilesProcessed++;
            }
            catch (Exception ex)
            {
                result.Errors++;
                result.ErrorMessages.Add($"{filePath}: {ex.Message}");
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        result.Duration = DateTime.UtcNow - startedAt;
        return result;
    }

    private IEnumerable<string> EnumerateFilesSkippingExcluded(string rootPath)
    {
        var stack = new Stack<string>();
        stack.Push(rootPath);

        while (stack.Count > 0)
        {
            var dir = stack.Pop();

            IEnumerable<string> subDirs;
            try
            {
                subDirs = Directory.EnumerateDirectories(dir);
            }
            catch { continue; }

            foreach (var sub in subDirs)
            {
                var name = Path.GetFileName(sub);
                if (_excludedFolders.Contains(name)) continue;
                stack.Push(sub);
            }

            IEnumerable<string> filesInDir;
            try
            {
                filesInDir = Directory.EnumerateFiles(dir);
            }
            catch { continue; }

            foreach (var file in filesInDir)
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (SupportedExtensions.Contains(ext))
                    yield return file;
            }
        }
    }

    private async Task ProcessFileAsync(string filePath, Peer peer, ScanResult result, CancellationToken ct)
    {
        var existing = await _db.TrackFiles
            .FirstOrDefaultAsync(f => f.PeerId == peer.Id && f.FilePath == filePath, ct);

        var meta = _metadataReader.Read(filePath);
        if (meta is null) return;

        var fileInfo = new FileInfo(filePath);
        var hash = await _hasher.ComputeHashAsync(filePath, ct);

        if (existing is not null)
        {
            existing.FileHash = hash;
            existing.FileSize = fileInfo.Length;
            existing.Bitrate = meta.Bitrate;
            existing.SampleRate = meta.SampleRate;
            existing.BitDepth = meta.BitDepth;
            existing.Channels = meta.Channels;
            existing.Format = meta.Format;
            existing.UpdatedAt = DateTime.UtcNow;
            result.TracksUpdated++;
            return;
        }

        var artistName = meta.AlbumArtist ?? meta.Artist ?? "Unknown Artist";
        var artist = await _db.Artists.FirstOrDefaultAsync(a => a.Name == artistName, ct);
        if (artist is null)
        {
            artist = new Artist { Name = artistName };
            _db.Artists.Add(artist);
            await _db.SaveChangesAsync(ct);
        }

        var albumTitle = string.IsNullOrWhiteSpace(meta.Album) ? "Unknown Album" : meta.Album!;
        var album = await _db.Albums
            .FirstOrDefaultAsync(a => a.ArtistId == artist.Id
                                      && a.Title == albumTitle
                                      && a.ReleaseYear == meta.Year, ct);
        if (album is null)
        {
            album = new Album
            {
                Title = albumTitle,
                ReleaseYear = meta.Year,
                ArtistId = artist.Id
            };
            _db.Albums.Add(album);
            await _db.SaveChangesAsync(ct);
        }

        // Fallback: если title пустой — используем имя файла
        var trackTitle = string.IsNullOrWhiteSpace(meta.Title)
            ? Path.GetFileNameWithoutExtension(filePath)
            : meta.Title;

        var trackNumber = meta.TrackNumber ?? 0;
        var discNumber = meta.DiscNumber ?? 1;

        // ВАЖНО: dedup-ключ теперь включает Title
        var track = await _db.Tracks
            .FirstOrDefaultAsync(t => t.AlbumId == album.Id
                                      && t.DiscNumber == discNumber
                                      && t.TrackNumber == trackNumber
                                      && t.Title == trackTitle, ct);

        if (track is null)
        {
            track = new Track
            {
                Title = trackTitle,
                Duration = meta.Duration,
                TrackNumber = trackNumber,
                DiscNumber = discNumber,
                AlbumId = album.Id,
                ArtistId = artist.Id
            };
            _db.Tracks.Add(track);
            await _db.SaveChangesAsync(ct);
            result.TracksCreated++;
        }

        var trackFile = new TrackFile
        {
            TrackId = track.Id,
            PeerId = peer.Id,
            FilePath = filePath,
            FileHash = hash,
            FileSize = fileInfo.Length,
            Format = meta.Format,
            Bitrate = meta.Bitrate,
            SampleRate = meta.SampleRate,
            BitDepth = meta.BitDepth,
            Channels = meta.Channels
        };
        _db.TrackFiles.Add(trackFile);
    }
}