using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;
using Resonanse.Domain.Enums;

namespace Resonanse.Application.Services;

public class LibraryQueryService : ILibraryQueryService
{
    private readonly IResonanseDbContext _db;
    private readonly ICurrentUser _currentUser; 

    public LibraryQueryService(IResonanseDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }
    // ============================================================
    // TRACKS
    // ============================================================

    public async Task<PagedResult<TrackDto>> GetTracksAsync(TrackQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var userId = _currentUser.UserId;

        var q = _db.Tracks.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLowerInvariant();
            q = q.Where(t => t.Title.ToLower().Contains(s));
        }

        if (query.ArtistId.HasValue)
            q = q.Where(t => t.ArtistId == query.ArtistId.Value);

        if (query.AlbumId.HasValue)
            q = q.Where(t => t.AlbumId == query.AlbumId.Value);

        if (query.Formats is { Count: > 0 })
        {
            var parsed = query.Formats
                .Select(f => Enum.TryParse<AudioFormat>(f, true, out var fmt) ? fmt : (AudioFormat?)null)
                .Where(f => f.HasValue)
                .Select(f => f!.Value)
                .ToList();

            if (parsed.Count > 0)
                q = q.Where(t => t.Files.Any(f => parsed.Contains(f.Format)));
        }

        if (query.MinBitDepth.HasValue)
            q = q.Where(t => t.Files.Any(f => f.BitDepth >= query.MinBitDepth.Value));

        if (query.MinSampleRate.HasValue)
            q = q.Where(t => t.Files.Any(f => f.SampleRate >= query.MinSampleRate.Value));

        if (query.HasCover.HasValue)
        {
            q = query.HasCover.Value
                ? q.Where(t => t.Album.CoverPath != null)
                : q.Where(t => t.Album.CoverPath == null);
        }

        q = ApplyTrackSort(q, query.SortBy, query.SortDir);

        var total = await q.CountAsync(ct);

        var raw = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Duration,
                t.TrackNumber,
                t.DiscNumber,
                t.ArtistId,
                ArtistName = t.Artist.Name,
                t.AlbumId,
                AlbumTitle = t.Album.Title,
                HasCover = t.Album.CoverPath != null,
                Formats = t.Files.Select(f => f.Format).ToList(),
                Sources = t.Files.Select(f => f.Source).ToList(),
                // ВЫЧИСЛЕНИЕ ISFAVORITE
                IsFavorite = userId.HasValue && _db.UserFavorites.Any(uf => uf.UserId == userId.Value && uf.TrackId == t.Id)
            })
            .ToListAsync(ct);

        var items = raw.Select(t => new TrackDto
        {
            Id = t.Id,
            Title = t.Title,
            Duration = t.Duration,
            TrackNumber = t.TrackNumber,
            DiscNumber = t.DiscNumber,
            ArtistId = t.ArtistId,
            ArtistName = t.ArtistName,
            AlbumId = t.AlbumId,
            AlbumTitle = t.AlbumTitle,
            HasCover = t.HasCover,
            Formats = t.Formats.Select(f => f.ToString()).Distinct().ToList(),
            BestFormat = TrackFormatHelper.PickBest(t.Formats)?.ToString(),
            Source = PickPrimarySource(t.Sources),
            IsFavorite = t.IsFavorite
        }).ToList();

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
        var userId = _currentUser.UserId;

        var raw = await _db.Tracks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Duration,
                t.TrackNumber,
                t.DiscNumber,
                t.ArtistId,
                ArtistName = t.Artist.Name,
                t.AlbumId,
                AlbumTitle = t.Album.Title,
                HasCover = t.Album.CoverPath != null,
                IsFavorite = userId.HasValue && _db.UserFavorites.Any(uf => uf.UserId == userId.Value && uf.TrackId == t.Id),
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
                    FileHash = f.FileHash,
                    Source = f.Source,
                    ExternalId = f.ExternalId,
                    ExpiresAt = f.ExpiresAt,
                    IsPinnedByUser = f.IsPinnedByUser
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (raw is null) return null;

        return new TrackDetailsDto
        {
            Id = raw.Id,
            Title = raw.Title,
            Duration = raw.Duration,
            TrackNumber = raw.TrackNumber,
            DiscNumber = raw.DiscNumber,
            ArtistId = raw.ArtistId,
            ArtistName = raw.ArtistName,
            AlbumId = raw.AlbumId,
            AlbumTitle = raw.AlbumTitle,
            HasCover = raw.HasCover,
            Formats = raw.Files.Select(f => f.Format.ToString()).Distinct().ToList(),
            BestFormat = TrackFormatHelper.PickBest(raw.Files.Select(f => f.Format))?.ToString(),
            Source = PickPrimarySource(raw.Files.Select(f => f.Source)),
            IsFavorite = raw.IsFavorite,
            Files = raw.Files
        };
    }

    // ============================================================
    // FAVORITES
    // ============================================================

    public async Task<PagedResult<TrackDto>> GetFavoritesAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
            return new PagedResult<TrackDto> { Items = new(), TotalCount = 0 };

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var q = _db.UserFavorites
            .AsNoTracking()
            .Where(uf => uf.UserId == userId.Value)
            .Select(uf => uf.Track)
            .AsQueryable();

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
                AlbumTitle = t.Album.Title,
                HasCover = t.Album.CoverPath != null,
                Formats = t.Files.Select(f => f.Format.ToString()).Distinct().ToList(),
                BestFormat = t.Files.OrderByDescending(f => f.BitDepth).Select(f => f.Format.ToString()).FirstOrDefault(),
                Source = "Local",
                IsFavorite = true // Мы точно знаем, что они любимые
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

    // ============================================================
    // ALBUMS
    // ============================================================

    public async Task<PagedResult<AlbumDto>> GetAlbumsAsync(AlbumQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var q = _db.Albums.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLowerInvariant();
            q = q.Where(a => a.Title.ToLower().Contains(s));
        }

        if (query.ArtistId.HasValue)
            q = q.Where(a => a.ArtistId == query.ArtistId.Value);

        if (query.YearFrom.HasValue)
            q = q.Where(a => a.ReleaseYear >= query.YearFrom.Value);

        if (query.YearTo.HasValue)
            q = q.Where(a => a.ReleaseYear <= query.YearTo.Value);

        if (query.HasCover.HasValue)
        {
            q = query.HasCover.Value
                ? q.Where(a => a.CoverPath != null)
                : q.Where(a => a.CoverPath == null);
        }

        q = ApplyAlbumSort(q, query.SortBy, query.SortDir);

        var total = await q.CountAsync(ct);

        var items = await q
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
        var userId = _currentUser.UserId;

        var raw = await _db.Albums
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.ReleaseYear,
                a.CoverPath,
                a.ArtistId,
                ArtistName = a.Artist.Name,
                TrackCount = a.Tracks.Count,
                Tracks = a.Tracks
                    .OrderBy(t => t.DiscNumber)
                    .ThenBy(t => t.TrackNumber)
                    .Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Duration,
                        t.TrackNumber,
                        t.DiscNumber,
                        t.ArtistId,
                        ArtistName = t.Artist.Name,
                        t.AlbumId,
                        AlbumTitle = t.Album.Title,
                        HasCover = t.Album.CoverPath != null,
                        Formats = t.Files.Select(f => f.Format).ToList(),
                        Sources = t.Files.Select(f => f.Source).ToList(),
                        IsFavorite = userId.HasValue && _db.UserFavorites.Any(uf => uf.UserId == userId.Value && uf.TrackId == t.Id)
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (raw is null) return null;

        return new AlbumDetailsDto
        {
            Id = raw.Id,
            Title = raw.Title,
            ReleaseYear = raw.ReleaseYear,
            CoverPath = raw.CoverPath,
            ArtistId = raw.ArtistId,
            ArtistName = raw.ArtistName,
            TrackCount = raw.TrackCount,
            Tracks = raw.Tracks.Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                Duration = t.Duration,
                TrackNumber = t.TrackNumber,
                DiscNumber = t.DiscNumber,
                ArtistId = t.ArtistId,
                ArtistName = t.ArtistName,
                AlbumId = t.AlbumId,
                AlbumTitle = t.AlbumTitle,
                HasCover = t.HasCover,
                Formats = t.Formats.Select(f => f.ToString()).Distinct().ToList(),
                BestFormat = TrackFormatHelper.PickBest(t.Formats)?.ToString(),
                Source = PickPrimarySource(t.Sources),
                IsFavorite = t.IsFavorite
            }).ToList()
        };
    }

    // ============================================================
    // ARTISTS
    // ============================================================

    public async Task<PagedResult<ArtistDto>> GetArtistsAsync(ArtistQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var q = _db.Artists.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLowerInvariant();
            q = q.Where(a => a.Name.ToLower().Contains(s));
        }

        q = ApplyArtistSort(q, query.SortBy, query.SortDir);

        var total = await q.CountAsync(ct);

        var items = await q
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

    // ============================================================
    // SEARCH
    // ============================================================

    public async Task<SearchResultDto> SearchAsync(string query, int limit, CancellationToken ct = default)
    {
        var result = new SearchResultDto { Query = query };
        if (string.IsNullOrWhiteSpace(query)) return result;

        limit = Math.Clamp(limit, 1, 100);
        var s = query.Trim().ToLowerInvariant();
        var userId = _currentUser.UserId;

        var rawTracks = await _db.Tracks
            .AsNoTracking()
            .Where(t => t.Title.ToLower().Contains(s))
            .OrderBy(t => t.Title)
            .Take(limit)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Duration,
                t.TrackNumber,
                t.DiscNumber,
                t.ArtistId,
                ArtistName = t.Artist.Name,
                t.AlbumId,
                AlbumTitle = t.Album.Title,
                HasCover = t.Album.CoverPath != null,
                Formats = t.Files.Select(f => f.Format).ToList(),
                Sources = t.Files.Select(f => f.Source).ToList(),
                IsFavorite = userId.HasValue && _db.UserFavorites.Any(uf => uf.UserId == userId.Value && uf.TrackId == t.Id)
            })
            .ToListAsync(ct);

        result.Tracks = rawTracks.Select(t => new TrackDto
        {
            Id = t.Id,
            Title = t.Title,
            Duration = t.Duration,
            TrackNumber = t.TrackNumber,
            DiscNumber = t.DiscNumber,
            ArtistId = t.ArtistId,
            ArtistName = t.ArtistName,
            AlbumId = t.AlbumId,
            AlbumTitle = t.AlbumTitle,
            HasCover = t.HasCover,
            Formats = t.Formats.Select(f => f.ToString()).Distinct().ToList(),
            BestFormat = TrackFormatHelper.PickBest(t.Formats)?.ToString(),
            Source = PickPrimarySource(t.Sources),
            IsFavorite = t.IsFavorite
        }).ToList();

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

    // ============================================================
    // HELPERS
    // ============================================================

    private static string PickPrimarySource(IEnumerable<TrackFileSource> sources)
    {
        var set = sources.ToHashSet();
        if (set.Contains(TrackFileSource.Local)) return "Local";
        if (set.Contains(TrackFileSource.Peer)) return "Peer";
        if (set.Contains(TrackFileSource.ArchiveOrg)) return "ArchiveOrg";
        if (set.Contains(TrackFileSource.YouTube)) return "YouTube";
        return "Other";
    }

    private static IQueryable<Resonanse.Domain.Entities.Track> ApplyTrackSort(
        IQueryable<Resonanse.Domain.Entities.Track> q,
        TrackSortField sortBy,
        SortDirection dir)
    {
        var desc = dir == SortDirection.Desc;

        return sortBy switch
        {
            TrackSortField.Title => desc
                ? q.OrderByDescending(t => t.Title)
                : q.OrderBy(t => t.Title),

            TrackSortField.Artist => desc
                ? q.OrderByDescending(t => t.Artist.Name).ThenBy(t => t.Title)
                : q.OrderBy(t => t.Artist.Name).ThenBy(t => t.Title),

            TrackSortField.Album => desc
                ? q.OrderByDescending(t => t.Album.Title).ThenBy(t => t.DiscNumber).ThenBy(t => t.TrackNumber)
                : q.OrderBy(t => t.Album.Title).ThenBy(t => t.DiscNumber).ThenBy(t => t.TrackNumber),

            TrackSortField.Duration => desc
                ? q.OrderByDescending(t => t.Duration)
                : q.OrderBy(t => t.Duration),

            TrackSortField.TrackNumber => desc
                ? q.OrderByDescending(t => t.TrackNumber)
                : q.OrderBy(t => t.TrackNumber),

            TrackSortField.CreatedAt => desc
                ? q.OrderByDescending(t => t.CreatedAt)
                : q.OrderBy(t => t.CreatedAt),

            _ => q.OrderBy(t => t.Title)
        };
    }

    private static IQueryable<Resonanse.Domain.Entities.Album> ApplyAlbumSort(
        IQueryable<Resonanse.Domain.Entities.Album> q,
        AlbumSortField sortBy,
        SortDirection dir)
    {
        var desc = dir == SortDirection.Desc;

        return sortBy switch
        {
            AlbumSortField.Title => desc
                ? q.OrderByDescending(a => a.Title)
                : q.OrderBy(a => a.Title),

            AlbumSortField.Artist => desc
                ? q.OrderByDescending(a => a.Artist.Name).ThenBy(a => a.Title)
                : q.OrderBy(a => a.Artist.Name).ThenBy(a => a.Title),

            AlbumSortField.ReleaseYear => desc
                ? q.OrderBy(a => a.ReleaseYear == null).ThenByDescending(a => a.ReleaseYear).ThenBy(a => a.Title)
                : q.OrderBy(a => a.ReleaseYear == null).ThenBy(a => a.ReleaseYear).ThenBy(a => a.Title),

            AlbumSortField.TrackCount => desc
                ? q.OrderByDescending(a => a.Tracks.Count).ThenBy(a => a.Title)
                : q.OrderBy(a => a.Tracks.Count).ThenBy(a => a.Title),

            AlbumSortField.CreatedAt => desc
                ? q.OrderByDescending(a => a.CreatedAt)
                : q.OrderBy(a => a.CreatedAt),

            _ => q.OrderBy(a => a.Title)
        };
    }

    private static IQueryable<Resonanse.Domain.Entities.Artist> ApplyArtistSort(
        IQueryable<Resonanse.Domain.Entities.Artist> q,
        ArtistSortField sortBy,
        SortDirection dir)
    {
        var desc = dir == SortDirection.Desc;

        return sortBy switch
        {
            ArtistSortField.Name => desc
                ? q.OrderByDescending(a => a.Name)
                : q.OrderBy(a => a.Name),

            ArtistSortField.AlbumCount => desc
                ? q.OrderByDescending(a => a.Albums.Count).ThenBy(a => a.Name)
                : q.OrderBy(a => a.Albums.Count).ThenBy(a => a.Name),

            ArtistSortField.TrackCount => desc
                ? q.OrderByDescending(a => a.Tracks.Count).ThenBy(a => a.Name)
                : q.OrderBy(a => a.Tracks.Count).ThenBy(a => a.Name),

            ArtistSortField.CreatedAt => desc
                ? q.OrderByDescending(a => a.CreatedAt)
                : q.OrderBy(a => a.CreatedAt),

            _ => q.OrderBy(a => a.Name)
        };
    }
}