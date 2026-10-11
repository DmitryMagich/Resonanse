using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public class PublicAlbumService : IPublicAlbumService
{
    private readonly IResonanseDbContext _db;

    public PublicAlbumService(IResonanseDbContext db)
    {
        _db = db;
    }

    public async Task<PublicAlbumDto?> GetTestAlbumAsync(CancellationToken ct = default)
    {
        var album = await _db.Albums
            .AsNoTracking()
            .Where(a => a.IsTestAlbum)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.ReleaseYear,
                a.CoverPath,
                ArtistName = a.Artist.Name
            })
            .FirstOrDefaultAsync(ct);

        if (album is null) return null;

        var tracks = await _db.Tracks
            .AsNoTracking()
            .Where(t => t.AlbumId == album.Id)
            .OrderBy(t => t.DiscNumber)
            .ThenBy(t => t.TrackNumber)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Duration,
                t.TrackNumber,
                BestFormat = t.Files
                    .OrderByDescending(f => f.BitDepth ?? 0)
                    .ThenByDescending(f => f.SampleRate)
                    .Select(f => f.Format.ToString())
                    .FirstOrDefault(),
                HasStream = t.Files.Any()
            })
            .ToListAsync(ct);

        return new PublicAlbumDto
        {
            Id = album.Id,
            Title = album.Title,
            ReleaseYear = album.ReleaseYear,
            CoverPath = album.CoverPath,
            ArtistName = album.ArtistName,
            Tracks = tracks.Select(t => new PublicTrackDto
            {
                Id = t.Id,
                Title = t.Title,
                Duration = t.Duration,
                TrackNumber = t.TrackNumber,
                Format = t.BestFormat,
                HasStream = t.HasStream
            }).ToList()
        };
    }
}