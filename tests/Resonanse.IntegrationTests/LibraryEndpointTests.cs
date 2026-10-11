using System.Net;
using System.Net.Http.Json;
using Resonanse.Application.Dtos;

namespace Resonanse.IntegrationTests;

[Collection("Integration")]
public class LibraryEndpointTests : IntegrationTestBase
{
    public LibraryEndpointTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Scan_NoAuth_Returns401()
    {
        ClearAuthorization();

        var response = await Client.PostAsync("/api/library/scan", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Scan_EnqueuesJob_ReturnsAccepted()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"resonanse-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            await AuthorizeAsync();

            var response = await Client.PostAsync(
                $"/api/library/scan?path={Uri.EscapeDataString(tempDir)}", null);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            var job = await response.Content.ReadFromJsonAsync<ScanJobDto>();
            Assert.NotNull(job);
            Assert.NotEqual(Guid.Empty, job!.Id);
            Assert.Equal("manual", job.Trigger);
            Assert.Equal(tempDir, job.Path);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_NonexistentPath_ReturnsConflict()
    {
        await AuthorizeAsync();

        var response = await Client.PostAsync(
            "/api/library/scan?path=/nonexistent/folder/xyz-123", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ScanStatus_NonexistentJob_Returns404()
    {
        await AuthorizeAsync();

        var response = await Client.GetAsync($"/api/library/scan/status/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ScanHistory_WithAuth_Returns200()
    {
        await AuthorizeAsync();

        var response = await Client.GetAsync("/api/library/scan/history?limit=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var history = await response.Content.ReadFromJsonAsync<List<ScanJobDto>>();
        Assert.NotNull(history);
    }
}