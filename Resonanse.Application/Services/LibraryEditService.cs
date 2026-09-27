using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public class LibraryEditService : ILibraryEditService
{
    private readonly IResonanseDbContext _db;

    public LibraryEditService(IResonanseDbContext db)
    {
        _db = db;
    }

    public async Task<TrackDto?> UpdateTrackAsync(Guid id, UpdateTrackRequest r, CancellationToken ct = default)
    {
        var track = await _db.Tracks.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (track is null) return null;

        if (r.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(r.Title))
                throw new InvalidOperationException("Title cannot be empty.");
            track.Title = r.Title.Trim();
        }

        if (r.TrackNumber.HasValue)
        {
            if (r.TrackNumber.Value < 0)
                throw new InvalidOperationException("TrackNumber cannot be negative.");
            track.TrackNumber = r.TrackNumber.Value;
        }

        if (r.DiscNumber.HasValue)
        {
            if (r.DiscNumber.Value < 1)
                throw new InvalidOperationException("DiscNumber must be at least 1.");
            track.DiscNumber = r.DiscNumber.Value;
        }

        if (r.ArtistId.HasValue)
        {
            if (!await _db.Artists.AnyAsync(a => a.Id == r.ArtistId.Value, ct))
                throw new InvalidOperationException($"Artist {r.ArtistId} not found.");
            track.ArtistId = r.ArtistId.Value;
        }

        if (r.AlbumId.HasValue)
        {
            if (!await _db.Albums.AnyAsync(a => a.Id == r.AlbumId.Value, ct))
                throw new InvalidOperationException($"Album {r.AlbumId} not found.");
            track.AlbumId = r.AlbumId.Value;
        }

        track.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await _db.Tracks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                Duration = t.Duration,
                TrackNumber = t.TrackNumber,
                DiscNumber = t.DiscNumber,
                ArtistId = t.ArtistId,
                ArtistName = t.Artist.Name,
                AlbumId = t.AlbumId,
                AlbumTitle = t.Album.Title
            })
            .FirstAsync(ct);
    }

    // ---------- ALBUM ----------

    public async Task<AlbumDto?> UpdateAlbumAsync(Guid id, UpdateAlbumRequest r, CancellationToken ct = default)
    {
        var album = await _db.Albums.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (album is null) return null;

        if (r.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(r.Title))
                throw new InvalidOperationException("Title cannot be empty.");
            album.Title = r.Title.Trim();
        }

        if (r.ReleaseYear.HasValue)
        {
            var year = r.ReleaseYear.Value;
            if (year < 1900 || year > DateTime.UtcNow.Year + 1)
                throw new InvalidOperationException($"ReleaseYear {year} is out of range.");
            album.ReleaseYear = year;
        }

        if (r.CoverPath is not null)
        {
            album.CoverPath = string.IsNullOrWhiteSpace(r.CoverPath) ? null : r.CoverPath.Trim();
        }

        if (r.ArtistId.HasValue)
        {
            if (!await _db.Artists.AnyAsync(a => a.Id == r.ArtistId.Value, ct))
                throw new InvalidOperationException($"Artist {r.ArtistId} not found.");
            album.ArtistId = r.ArtistId.Value;
        }

        album.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await _db.Albums
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AlbumDto
            {
                Id = a.Id,
                Title = a.Title,
                ReleaseYear = a.ReleaseYear,
                CoverPath = a.CoverPath,
                ArtistId = a.ArtistId,
                ArtistName = a.Artist.Name,
                TrackCount = a.Tracks.Count
            })
            .FirstAsync(ct);
    }

    // ---------- ARTIST ----------

    public async Task<ArtistDto?> UpdateArtistAsync(Guid id, UpdateArtistRequest r, CancellationToken ct = default)
    {
        var artist = await _db.Artists.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (artist is null) return null;

        if (r.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(r.Name))
                throw new InvalidOperationException("Name cannot be empty.");
            artist.Name = r.Name.Trim();
        }

        if (r.Bio is not null)
            artist.Bio = string.IsNullOrWhiteSpace(r.Bio) ? null : r.Bio;

        if (r.ImagePath is not null)
            artist.ImagePath = string.IsNullOrWhiteSpace(r.ImagePath) ? null : r.ImagePath.Trim();

        artist.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await _db.Artists
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new ArtistDto
            {
                Id = a.Id,
                Name = a.Name,
                ImagePath = a.ImagePath,
                AlbumCount = a.Albums.Count,
                TrackCount = a.Tracks.Count
            })
            .FirstAsync(ct);
    }
}