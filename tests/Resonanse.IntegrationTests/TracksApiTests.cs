using System.Net;
using System.Net.Http.Json;
using Resonanse.Application.Dtos;

namespace Resonanse.IntegrationTests;

[Collection("Integration")]
public class TracksApiTests : IntegrationTestBase
{
    public TracksApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetTracks_EmptyDb_ReturnsEmptyPage()
    {
        await WithDbAsync(TestDataSeeder.CleanupTracksAsync);
        await AuthorizeAsync();

        var response = await Client.GetAsync("/api/tracks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<TrackDto>>();
        Assert.NotNull(page);
        Assert.Empty(page!.Items);
        Assert.Equal(0, page.TotalCount);
    }

    [Fact]
    public async Task GetTracks_WithSeed_ReturnsTrack()
    {
        await WithDbAsync(TestDataSeeder.CleanupTracksAsync);
        var (_, _, track, _, _) = await WithDbAsync(db => TestDataSeeder.SeedOneTrackAsync(db, "Riders in the Sky"));
        await AuthorizeAsync();

        var response = await Client.GetAsync("/api/tracks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<TrackDto>>();
        Assert.NotNull(page);
        Assert.Single(page!.Items);
        Assert.Equal(track.Id, page.Items[0].Id);
        Assert.Equal("Riders in the Sky", page.Items[0].Title);
    }

    [Fact]
    public async Task GetTrackById_ReturnsDetails()
    {
        await WithDbAsync(TestDataSeeder.CleanupTracksAsync);
        var (_, _, track, _, _) = await WithDbAsync(db => TestDataSeeder.SeedOneTrackAsync(db));
        await AuthorizeAsync();

        var response = await Client.GetAsync($"/api/tracks/{track.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var details = await response.Content.ReadFromJsonAsync<TrackDetailsDto>();
        Assert.NotNull(details);
        Assert.Single(details!.Files);
        Assert.Equal(24, details.Files[0].BitDepth);
    }

    [Fact]
    public async Task GetTrackById_Nonexistent_Returns404()
    {
        await AuthorizeAsync();

        var response = await Client.GetAsync($"/api/tracks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PatchTrack_UpdatesTitle()
    {
        await WithDbAsync(TestDataSeeder.CleanupTracksAsync);
        var (_, _, track, _, _) = await WithDbAsync(db => TestDataSeeder.SeedOneTrackAsync(db));
        await AuthorizeAsync();

        var body = new { title = "Updated Title" };
        var response = await Client.PatchAsJsonAsync($"/api/tracks/{track.Id}", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TrackDto>();
        Assert.Equal("Updated Title", updated!.Title);
    }

    [Fact]
    public async Task PatchTrack_EmptyTitle_Returns400()
    {
        await WithDbAsync(TestDataSeeder.CleanupTracksAsync);
        var (_, _, track, _, _) = await WithDbAsync(db => TestDataSeeder.SeedOneTrackAsync(db));
        await AuthorizeAsync();

        var body = new { title = "   " };
        var response = await Client.PatchAsJsonAsync($"/api/tracks/{track.Id}", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_FindsSeededTrack()
    {
        await WithDbAsync(TestDataSeeder.CleanupTracksAsync);
        await WithDbAsync(db => TestDataSeeder.SeedOneTrackAsync(db, "Test Track"));

        await AuthorizeAsync();
        var response = await Client.GetAsync("/api/search?q=test");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SearchResultDto>();
        Assert.NotNull(result);
        Assert.Single(result!.Tracks);
        Assert.Single(result.Albums);
        Assert.Single(result.Artists);
    }
}