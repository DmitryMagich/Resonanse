using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public class LibraryQueryService : ILibraryQueryService
{
    private readonly IResonanseDbContext _db;

    public LibraryQueryService(IResonanseDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<TrackDto>> GetTracksAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var q = _db.Tracks.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(t => t.Title.ToLower().Contains(s));
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderBy(t => t.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
            .ToListAsync(ct);

        return new PagedResult<TrackDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<TrackDetailsDto?> GetTrackAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Tracks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TrackDetailsDto
            {
                Id = t.Id,
                Title = t.Title,
                Duration = t.Duration,
                TrackNumber = t.TrackNumber,
                DiscNumber = t.DiscNumber,
                ArtistId = t.ArtistId,
                ArtistName = t.Artist.Name,
                AlbumId = t.AlbumId,
                AlbumTitle = t.Album.Title,
                Files = t.Files.Select(f => new TrackFileDto
                {
                    Id = f.Id,
                    PeerId = f.PeerId,
                    PeerName = f.Peer.Name,
                    Format = f.Format,
                    Bitrate = f.Bitrate,
                    SampleRate = f.SampleRate,
                    BitDepth = f.BitDepth,
                    Channels = f.Channels,
                    FileSize = f.FileSize,
                    FileHash = f.FileHash
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<AlbumDto>> GetAlbumsAsync(int page, int pageSize, Guid? artistId, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var q = _db.Albums.AsNoTracking().AsQueryable();

        if (artistId.HasValue)
            q = q.Where(a => a.ArtistId == artistId.Value);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderBy(a => a.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
            .ToListAsync(ct);

        return new PagedResult<AlbumDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<AlbumDetailsDto?> GetAlbumAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Albums
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AlbumDetailsDto
            {
                Id = a.Id,
                Title = a.Title,
                ReleaseYear = a.ReleaseYear,
                CoverPath = a.CoverPath,
                ArtistId = a.ArtistId,
                ArtistName = a.Artist.Name,
                TrackCount = a.Tracks.Count,
                Tracks = a.Tracks
                    .OrderBy(t => t.DiscNumber)
                    .ThenBy(t => t.TrackNumber)
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
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<ArtistDto>> GetArtistsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var q = _db.Artists.AsNoTracking();

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArtistDto
            {
                Id = a.Id,
                Name = a.Name,
                ImagePath = a.ImagePath,
                AlbumCount = a.Albums.Count,
                TrackCount = a.Tracks.Count
            })
            .ToListAsync(ct);

        return new PagedResult<ArtistDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<ArtistDetailsDto?> GetArtistAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Artists
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new ArtistDetailsDto
            {
                Id = a.Id,
                Name = a.Name,
                ImagePath = a.ImagePath,
                Bio = a.Bio,
                AlbumCount = a.Albums.Count,
                TrackCount = a.Tracks.Count,
                Albums = a.Albums
                    .OrderBy(al => al.ReleaseYear)
                    .ThenBy(al => al.Title)
                    .Select(al => new AlbumDto
                    {
                        Id = al.Id,
                        Title = al.Title,
                        ReleaseYear = al.ReleaseYear,
                        CoverPath = al.CoverPath,
                        ArtistId = al.ArtistId,
                        ArtistName = al.Artist.Name,
                        TrackCount = al.Tracks.Count
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SearchResultDto> SearchAsync(string query, int limit, CancellationToken ct = default)
    {
        var result = new SearchResultDto { Query = query };
        if (string.IsNullOrWhiteSpace(query)) return result;

        limit = Math.Clamp(limit, 1, 100);
        var s = query.Trim().ToLower();

        result.Tracks = await _db.Tracks
            .AsNoTracking()
            .Where(t => t.Title.ToLower().Contains(s))
            .OrderBy(t => t.Title)
            .Take(limit)
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
            .ToListAsync(ct);

        result.Artists = await _db.Artists
            .AsNoTracking()
            .Where(a => a.Name.ToLower().Contains(s))
            .OrderBy(a => a.Name)
            .Take(limit)
            .Select(a => new ArtistDto
            {
                Id = a.Id,
                Name = a.Name,
                ImagePath = a.ImagePath,
                AlbumCount = a.Albums.Count,
                TrackCount = a.Tracks.Count
            })
            .ToListAsync(ct);

        result.Albums = await _db.Albums
            .AsNoTracking()
            .Where(a => a.Title.ToLower().Contains(s))
            .OrderBy(a => a.Title)
            .Take(limit)
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
            .ToListAsync(ct);

        return result;
    }
}