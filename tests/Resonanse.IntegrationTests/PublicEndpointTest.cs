using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Dtos;
using Resonanse.Application.Dtos.Auth;
using Resonanse.Domain.Entities;

namespace Resonanse.IntegrationTests;

[Collection("Integration")]
public class PublicEndpointTests : IntegrationTestBase
{
    public PublicEndpointTests(CustomWebApplicationFactory factory) : base(factory) { }

    // ============================================================
    // PUBLIC TEST ALBUM
    // ============================================================

    [Fact]
    public async Task TestAlbum_NoAuth_Returns200()
    {
        var albumId = await SeedTestAlbumAsync();

        var response = await Client.GetAsync("/api/public/test-album");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var album = await response.Content.ReadFromJsonAsync<PublicAlbumDto>();
        Assert.NotNull(album);
        Assert.Equal(albumId, album!.Id);
        Assert.Single(album.Tracks);
    }

    [Fact]
    public async Task TestAlbum_NoTestAlbumConfigured_Returns404()
    {
        await WithDbAsync(TestDataSeeder.CleanupTracksAsync);

        var response = await Client.GetAsync("/api/public/test-album");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ============================================================
    // STREAMING ACCESS
    // ============================================================

    [Fact]
    public async Task Stream_TrackInTestAlbum_NoAuth_Returns200()
    {
        var (_, trackId, _) = await SeedTestAlbumWithFileAsync();

        var response = await Client.GetAsync($"/api/stream/track/{trackId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Stream_TrackNotInTestAlbum_NoAuth_Returns403()
    {
        var (_, _, trackId, _, _) = await SeedTrackWithRealFileAsync();

        var response = await Client.GetAsync($"/api/stream/track/{trackId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Stream_TrackNotInTestAlbum_WithAdmin_Returns200()
    {
        var (_, _, trackId, _, _) = await SeedTrackWithRealFileAsync();

        await AuthorizeAsync();
        var response = await Client.GetAsync($"/api/stream/track/{trackId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ============================================================
    // AUTHORIZED ENDPOINTS WITHOUT AUTH
    // ============================================================

    [Fact]
    public async Task Tracks_NoAuth_Returns401()
    {
        ClearAuthorization();
        var response = await Client.GetAsync("/api/tracks");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Search_NoAuth_Returns401()
    {
        ClearAuthorization();
        var response = await Client.GetAsync("/api/search?q=test");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Albums_NoAuth_Returns401()
    {
        ClearAuthorization();
        var response = await Client.GetAsync("/api/albums");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Artists_NoAuth_Returns401()
    {
        ClearAuthorization();
        var response = await Client.GetAsync("/api/artists");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ============================================================
    // INVITE REQUESTS
    // ============================================================

    [Fact]
    public async Task CreateInviteRequest_NoAuth_Returns200()
    {
        var body = new CreateInviteRequestBody
        {
            Name = "Test Guest",
            Comment = "хочу",
            Contact = "@test"
        };

        var response = await Client.PostAsJsonAsync("/api/public/invite-requests", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<InviteRequestDto>();
        Assert.NotNull(created);
        Assert.Equal("Test Guest", created!.Name);
        Assert.Equal(0, (int)created.Status);
    }

    [Fact]
    public async Task CreateInviteRequest_EmptyName_Returns400()
    {
        var body = new CreateInviteRequestBody { Name = "" };

        var response = await Client.PostAsJsonAsync("/api/public/invite-requests", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListInviteRequests_NoAuth_Returns401()
    {
        ClearAuthorization();
        var response = await Client.GetAsync("/api/admin/invite-requests");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ApproveInviteRequest_CreatesInvite()
    {
        ClearAuthorization();
        var createResp = await Client.PostAsJsonAsync("/api/public/invite-requests",
            new CreateInviteRequestBody { Name = "Approval Test" });
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<InviteRequestDto>();
        Assert.NotNull(created);

        await AuthorizeAsync();
        var invitesBefore = await Client.GetFromJsonAsync<List<InviteDto>>("/api/admin/invites");

        var approveResp = await Client.PostAsync(
            $"/api/admin/invite-requests/{created!.Id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approveResp.StatusCode);

        var approved = await approveResp.Content.ReadFromJsonAsync<InviteRequestDto>();
        Assert.NotNull(approved);
        Assert.Equal(1, (int)approved!.Status);
        Assert.NotNull(approved.ProcessedAt);

        var invitesAfter = await Client.GetFromJsonAsync<List<InviteDto>>("/api/admin/invites");
        Assert.NotNull(invitesBefore);
        Assert.NotNull(invitesAfter);
        Assert.Equal(invitesBefore!.Count + 1, invitesAfter!.Count);
    }

    // ============================================================
    // MARK TEST ALBUM
    // ============================================================

    [Fact]
    public async Task MarkTestAlbum_ThenPublicReturnsIt()
    {
        var (_, album, _, _, _) = await ResetAndSeedAsync("Test");

        await AuthorizeAsync();
        var markResp = await Client.PostAsync($"/api/admin/albums/{album.Id}/mark-test", null);
        Assert.Equal(HttpStatusCode.OK, markResp.StatusCode);

        ClearAuthorization();
        var publicResp = await Client.GetAsync("/api/public/test-album");
        Assert.Equal(HttpStatusCode.OK, publicResp.StatusCode);

        var publicAlbum = await publicResp.Content.ReadFromJsonAsync<PublicAlbumDto>();
        Assert.NotNull(publicAlbum);
        Assert.Equal(album.Id, publicAlbum!.Id);
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private async Task<(Artist artist, Album album, Track track, Peer peer, TrackFile file)>
        ResetAndSeedAsync(string title = "Test Track")
    {
        return await WithDbAsync(async db =>
        {
            await TestDataSeeder.CleanupTracksAsync(db);
            return await TestDataSeeder.SeedOneTrackAsync(db, title);
        });
    }

    private async Task<Guid> SeedTestAlbumAsync()
    {
        var (_, album, _, _, _) = await ResetAndSeedAsync();

        await WithDbAsync(async db =>
        {
            var entity = await db.Albums.FirstAsync(a => a.Id == album.Id);
            entity.IsTestAlbum = true;
            await db.SaveChangesAsync();
        });

        return album.Id;
    }

    private async Task<(Guid albumId, Guid trackId, Guid fileId)> SeedTestAlbumWithFileAsync()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"resonanse-test-{Guid.NewGuid():N}.flac");
        await File.WriteAllBytesAsync(filePath, new byte[1024]);

        var (_, album, track, _, file) = await ResetAndSeedAsync("Test Track");

        await WithDbAsync(async db =>
        {
            var albumEntity = await db.Albums.FirstAsync(a => a.Id == album.Id);
            albumEntity.IsTestAlbum = true;

            var fileEntity = await db.TrackFiles.FirstAsync(f => f.Id == file.Id);
            fileEntity.FilePath = filePath;
            fileEntity.FileSize = 1024;

            await db.SaveChangesAsync();
        });

        return (album.Id, track.Id, file.Id);
    }

    /// <summary>
    /// Трек с реальным файлом на диске, но БЕЗ флага IsTestAlbum.
    /// Для проверки, что аноним не может стримить чужие треки.
    /// </summary>
    private async Task<(Guid albumId, Guid artistId, Guid trackId, Guid peerId, Guid fileId)>
        SeedTrackWithRealFileAsync()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"resonanse-other-{Guid.NewGuid():N}.flac");
        await File.WriteAllBytesAsync(filePath, new byte[1024]);

        var (artist, album, track, peer, file) = await ResetAndSeedAsync("Not Test Track");

        await WithDbAsync(async db =>
        {
            var fileEntity = await db.TrackFiles.FirstAsync(f => f.Id == file.Id);
            fileEntity.FilePath = filePath;
            fileEntity.FileSize = 1024;
            await db.SaveChangesAsync();
        });

        return (album.Id, artist.Id, track.Id, peer.Id, file.Id);
    }
}