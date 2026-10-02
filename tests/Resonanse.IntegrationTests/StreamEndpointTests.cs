using System.Net;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Resonanse.Domain.Enums;

namespace Resonanse.IntegrationTests;

[Collection("Integration")]
public class StreamEndpointTests : IntegrationTestBase
{
    public StreamEndpointTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Stream_NonexistentTrack_Returns404()
    {
        await AuthorizeAsync();

        var response = await Client.GetAsync($"/api/stream/track/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Stream_ValidFile_Returns200WithContentType()
    {
        var (trackId, _) = await SeedTrackWithRealFileAsync();
        await AuthorizeAsync();

        var response = await Client.GetAsync($"/api/stream/track/{trackId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("audio/flac", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(1024, response.Content.Headers.ContentLength);
        Assert.True(response.Headers.AcceptRanges.Contains("bytes"));
    }

    [Fact]
    public async Task Stream_RangeRequest_Returns206PartialContent()
    {
        var (trackId, _) = await SeedTrackWithRealFileAsync();
        await AuthorizeAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/stream/track/{trackId}");
        request.Headers.Range = new RangeHeaderValue(0, 99);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        var content = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(100, content.Length);
        Assert.NotNull(response.Content.Headers.ContentRange);
    }

    [Fact]
    public async Task Stream_TrackFileDirectId_Works()
    {
        var (_, fileId) = await SeedTrackWithRealFileAsync();
        await AuthorizeAsync();

        var response = await Client.GetAsync($"/api/stream/{fileId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ---------- helpers ----------

    /// <summary>
    /// Создаёт реальный файл в /tmp, пишет 1KB данных, регистрирует его как TrackFile.
    /// </summary>
    private async Task<(Guid trackId, Guid fileId)> SeedTrackWithRealFileAsync()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"resonanse-stream-test-{Guid.NewGuid():N}.flac");
        await File.WriteAllBytesAsync(filePath, new byte[1024]);

        var ids = await WithDbAsync(async db =>
        {
            db.TrackFiles.RemoveRange(db.TrackFiles);
            db.Tracks.RemoveRange(db.Tracks);
            db.Albums.RemoveRange(db.Albums);
            db.Artists.RemoveRange(db.Artists);
            await db.SaveChangesAsync();

            var (_, _, track, _, file) = await TestDataSeeder.SeedOneTrackAsync(db);

            // Обновляем путь к реальному файлу
            var dbFile = await db.TrackFiles.FirstAsync(f => f.Id == file.Id);
            dbFile.FilePath = filePath;
            dbFile.Format = AudioFormat.Flac;
            dbFile.FileSize = 1024;
            await db.SaveChangesAsync();

            return (track.Id, file.Id);
        });

        return ids;
    }
}