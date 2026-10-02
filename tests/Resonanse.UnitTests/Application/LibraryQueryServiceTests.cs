using Resonanse.Application.Services;
using Resonanse.Domain.Entities;
using Resonanse.Domain.Enums;
using Resonanse.Infrastructure.Persistence;
using Resonanse.UnitTests.Infrastructure;
using Xunit;

namespace Resonanse.UnitTests.Application;

public class LibraryQueryServiceTests
{
    [Fact]
    public async Task GetTracksAsync_ReturnsPagedResults()
    {
        using var db = TestDbContextFactory.Create();
        SeedManyTracks(db, 25);
        var service = new LibraryQueryService(db);

        var result = await service.GetTracksAsync(page: 1, pageSize: 10, search: null);

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task GetTracksAsync_SecondPage_ReturnsRemaining()
    {
        using var db = TestDbContextFactory.Create();
        SeedManyTracks(db, 25);
        var service = new LibraryQueryService(db);

        var result = await service.GetTracksAsync(page: 3, pageSize: 10, search: null);

        Assert.Equal(5, result.Items.Count);
    }

    [Fact]
    public async Task GetTracksAsync_SearchFiltersByTitle()
    {
        using var db = TestDbContextFactory.Create();
        SeedManyTracks(db, 10);
        var service = new LibraryQueryService(db);

        var result = await service.GetTracksAsync(page: 1, pageSize: 100, search: "Track 1");

        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetTrackAsync_ReturnsTrackWithFiles()
    {
        using var db = TestDbContextFactory.Create();
        var (_, _, track) = SeedBasic(db);
        var service = new LibraryQueryService(db);

        var result = await service.GetTrackAsync(track.Id);

        Assert.NotNull(result);
        Assert.Equal("Test Track", result!.Title);
        Assert.Single(result.Files);
    }

    [Fact]
    public async Task GetTrackAsync_Nonexistent_ReturnsNull()
    {
        using var db = TestDbContextFactory.Create();
        var service = new LibraryQueryService(db);

        var result = await service.GetTrackAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAlbumsAsync_FilterByArtistId()
    {
        using var db = TestDbContextFactory.Create();
        var (artist, _, _) = SeedBasic(db);

        var otherArtist = new Artist { Name = "Other" };
        var otherAlbum = new Album { Title = "Other Album", ArtistId = otherArtist.Id };
        db.Artists.Add(otherArtist);
        db.Albums.Add(otherAlbum);
        db.SaveChanges();

        var service = new LibraryQueryService(db);

        var result = await service.GetAlbumsAsync(page: 1, pageSize: 100, artistId: artist.Id);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(artist.Id, result.Items[0].ArtistId);
    }

    [Fact]
    public async Task GetArtistsAsync_ReturnsCounts()
    {
        using var db = TestDbContextFactory.Create();
        SeedBasic(db);
        var service = new LibraryQueryService(db);

        var result = await service.GetArtistsAsync(page: 1, pageSize: 100);

        Assert.Single(result.Items);
        Assert.Equal(1, result.Items[0].AlbumCount);
        Assert.Equal(1, result.Items[0].TrackCount);
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatches()
    {
        using var db = TestDbContextFactory.Create();
        SeedBasic(db);
        var service = new LibraryQueryService(db);

        var result = await service.SearchAsync("Test", limit: 10);

        Assert.Single(result.Tracks);
        Assert.Single(result.Albums);
        Assert.Single(result.Artists);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty()
    {
        using var db = TestDbContextFactory.Create();
        SeedBasic(db);
        var service = new LibraryQueryService(db);

        var result = await service.SearchAsync("", limit: 10);

        Assert.Empty(result.Tracks);
        Assert.Empty(result.Albums);
        Assert.Empty(result.Artists);
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
        var peer = new Peer { Name = "test-peer" };
        var file = new TrackFile
        {
            TrackId = track.Id,
            PeerId = peer.Id,
            FilePath = "/music/test.flac",
            FileHash = new string('a', 64),
            FileSize = 1000,
            Format = AudioFormat.Flac,
            Bitrate = 1000,
            SampleRate = 44100,
            BitDepth = 24,
            Channels = 2
        };

        db.Artists.Add(artist);
        db.Albums.Add(album);
        db.Tracks.Add(track);
        db.Peers.Add(peer);
        db.TrackFiles.Add(file);
        db.SaveChanges();

        return (artist, album, track);
    }

    private static void SeedManyTracks(ResonanseDbContext db, int count)
    {
        var artist = new Artist { Name = "Artist" };
        var album = new Album { Title = "Album", ArtistId = artist.Id };
        db.Artists.Add(artist);
        db.Albums.Add(album);
        db.SaveChanges();

        for (int i = 1; i <= count; i++)
        {
            db.Tracks.Add(new Track
            {
                Title = $"Track {i}",
                ArtistId = artist.Id,
                AlbumId = album.Id,
                TrackNumber = i,
                DiscNumber = 1,
                Duration = TimeSpan.FromSeconds(180)
            });
        }
        db.SaveChanges();
    }
}
