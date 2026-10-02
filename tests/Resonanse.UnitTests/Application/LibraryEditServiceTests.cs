using Resonanse.Application.Dtos;
using Resonanse.Application.Services;
using Resonanse.Domain.Entities;
using Resonanse.Infrastructure.Persistence;
using Resonanse.UnitTests.Infrastructure;
using Xunit;

namespace Resonanse.UnitTests.Application;

public class LibraryEditServiceTests
{
    [Fact]
    public async Task UpdateTrackAsync_UpdatesTitle()
    {
        using var db = TestDbContextFactory.Create();
        var (_, _, track) = SeedBasic(db);
        var service = new LibraryEditService(db);

        var result = await service.UpdateTrackAsync(track.Id, new UpdateTrackRequest { Title = "New Title" });

        Assert.NotNull(result);
        Assert.Equal("New Title", result!.Title);

        var fromDb = await db.Tracks.FindAsync(track.Id);
        Assert.Equal("New Title", fromDb!.Title);
        Assert.NotNull(fromDb.UpdatedAt);
    }

    [Fact]
    public async Task UpdateTrackAsync_NullFields_AreNotChanged()
    {
        using var db = TestDbContextFactory.Create();
        var (_, _, track) = SeedBasic(db);
        var service = new LibraryEditService(db);

        var originalTitle = track.Title;
        var originalTrackNumber = track.TrackNumber;

        await service.UpdateTrackAsync(track.Id, new UpdateTrackRequest { DiscNumber = 2 });

        var fromDb = await db.Tracks.FindAsync(track.Id);
        Assert.Equal(originalTitle, fromDb!.Title);
        Assert.Equal(originalTrackNumber, fromDb.TrackNumber);
        Assert.Equal(2, fromDb.DiscNumber);
    }

    [Fact]
    public async Task UpdateTrackAsync_EmptyTitle_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var (_, _, track) = SeedBasic(db);
        var service = new LibraryEditService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateTrackAsync(track.Id, new UpdateTrackRequest { Title = "   " }));
    }

    [Fact]
    public async Task UpdateTrackAsync_NonexistentId_ReturnsNull()
    {
        using var db = TestDbContextFactory.Create();
        var service = new LibraryEditService(db);

        var result = await service.UpdateTrackAsync(Guid.NewGuid(), new UpdateTrackRequest { Title = "X" });

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTrackAsync_NonexistentArtist_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var (_, _, track) = SeedBasic(db);
        var service = new LibraryEditService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateTrackAsync(track.Id, new UpdateTrackRequest { ArtistId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task UpdateTrackAsync_NegativeTrackNumber_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var (_, _, track) = SeedBasic(db);
        var service = new LibraryEditService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateTrackAsync(track.Id, new UpdateTrackRequest { TrackNumber = -1 }));
    }

    [Fact]
    public async Task UpdateAlbumAsync_UpdatesReleaseYear()
    {
        using var db = TestDbContextFactory.Create();
        var (_, album, _) = SeedBasic(db);
        var service = new LibraryEditService(db);

        var result = await service.UpdateAlbumAsync(album.Id, new UpdateAlbumRequest { ReleaseYear = 1949 });

        Assert.NotNull(result);
        Assert.Equal(1949, result!.ReleaseYear);
    }

    [Fact]
    public async Task UpdateAlbumAsync_InvalidYear_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var (_, album, _) = SeedBasic(db);
        var service = new LibraryEditService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAlbumAsync(album.Id, new UpdateAlbumRequest { ReleaseYear = 1800 }));
    }

    [Fact]
    public async Task UpdateArtistAsync_UpdatesName()
    {
        using var db = TestDbContextFactory.Create();
        var (artist, _, _) = SeedBasic(db);
        var service = new LibraryEditService(db);

        var result = await service.UpdateArtistAsync(artist.Id, new UpdateArtistRequest { Name = "New Artist Name" });

        Assert.NotNull(result);
        Assert.Equal("New Artist Name", result!.Name);
    }

    [Fact]
    public async Task UpdateArtistAsync_EmptyName_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var (artist, _, _) = SeedBasic(db);
        var service = new LibraryEditService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateArtistAsync(artist.Id, new UpdateArtistRequest { Name = "" }));
    }

    private static (Artist artist, Album album, Track track) SeedBasic(ResonanseDbContext db)
    {
        var artist = new Artist { Name = "Test Artist" };
        var album = new Album { Title = "Test Album", ArtistId = artist.Id, ReleaseYear = 2000 };
        var track = new Track
        {
            Title = "Test Track",
            ArtistId = artist.Id,
            AlbumId = album.Id,
            TrackNumber = 1,
            DiscNumber = 1,
            Duration = TimeSpan.FromSeconds(180)
        };

        db.Artists.Add(artist);
        db.Albums.Add(album);
        db.Tracks.Add(track);
        db.SaveChanges();

        return (artist, album, track);
    }
}
