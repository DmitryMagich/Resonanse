using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Metadata;

namespace Resonanse.Application.Services;

public class CoverService : ICoverService
{
    private static readonly string[] FolderCoverNames =
    {
        "cover.jpg", "cover.jpeg", "cover.png", "cover.webp",
        "folder.jpg", "folder.jpeg", "folder.png",
        "front.jpg", "front.jpeg", "front.png",
        "album.jpg", "album.jpeg", "album.png",
        "AlbumArt.jpg", "AlbumArt.png"
    };

    private readonly IResonanseDbContext _db;
    private readonly IMetadataReader _metadataReader;
    private readonly ICoverStorage _storage;

    public CoverService(IResonanseDbContext db, IMetadataReader metadataReader, ICoverStorage storage)
    {
        _db = db;
        _metadataReader = metadataReader;
        _storage = storage;
    }

    public async Task<string?> EnsureCoverForAlbumAsync(Guid albumId, string audioFilePath, CancellationToken ct = default)
    {
        var album = await _db.Albums.FirstOrDefaultAsync(a => a.Id == albumId, ct);
        if (album is null) return null;

        if (!string.IsNullOrWhiteSpace(album.CoverPath) && _storage.Exists(album.CoverPath))
            return album.CoverPath;

        var cover = SafeReadFromTags(audioFilePath) ?? SafeReadFromFolder(audioFilePath);
        if (cover is null) return null;

        var relPath = await _storage.SaveAsync(albumId, cover, ct);

        album.CoverPath = relPath;
        album.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return relPath;
    }

    private CoverMetadata? SafeReadFromTags(string audioFilePath)
    {
        try
        {
            return _metadataReader.ReadCover(audioFilePath);
        }
        catch
        {
            return null;
        }
    }

    private static CoverMetadata? SafeReadFromFolder(string audioFilePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(audioFilePath);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return null;

            foreach (var name in FolderCoverNames)
            {
                var candidate = Path.Combine(dir, name);
                if (!File.Exists(candidate)) continue;

                var ext = Path.GetExtension(candidate).ToLowerInvariant();
                var mime = ext switch
                {
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    ".jpeg" => "image/jpeg",
                    ".jpg" => "image/jpeg",
                    _ => "image/jpeg"
                };

                var bytes = File.ReadAllBytes(candidate);
                if (bytes.Length == 0) continue;

                return new CoverMetadata(bytes, mime);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}