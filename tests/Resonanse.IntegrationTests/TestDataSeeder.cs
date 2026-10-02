using Resonanse.Domain.Entities;
using Resonanse.Domain.Enums;
using Resonanse.Infrastructure.Persistence;

namespace Resonanse.IntegrationTests;

public static class TestDataSeeder
{
    public static async Task<(Artist artist, Album album, Track track, Peer peer, TrackFile file)> SeedOneTrackAsync(
        ResonanseDbContext db,
        string? trackTitle = "Test Track")
    {
        var peerName = $"test-peer-{Guid.NewGuid():N}";

        var artist = new Artist { Name = "Test Artist" };
        var album = new Album { Title = "Test Album", ArtistId = artist.Id, ReleaseYear = 2000 };
        var track = new Track
        {
            Title = trackTitle,
            ArtistId = artist.Id,
            AlbumId = album.Id,
            TrackNumber = 1,
            DiscNumber = 1,
            Duration = TimeSpan.FromSeconds(180)
        };
        var peer = new Peer { Name = peerName, IsOnline = true };
        var file = new TrackFile
        {
            TrackId = track.Id,
            PeerId = peer.Id,
            FilePath = "/tmp/test.flac",
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
        await db.SaveChangesAsync();

        return (artist, album, track, peer, file);
    }

    public static async Task CleanupTracksAsync(ResonanseDbContext db)
    {
        db.TrackFiles.RemoveRange(db.TrackFiles);
        db.Tracks.RemoveRange(db.Tracks);
        db.Albums.RemoveRange(db.Albums);
        db.Artists.RemoveRange(db.Artists);
        await db.SaveChangesAsync();
    }
}