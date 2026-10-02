using System.Net;
using System.Net.Http.Json;
using Resonanse.Application.Services;

namespace Resonanse.IntegrationTests;

[Collection("Integration")]
public class LibraryEndpointTests : IntegrationTestBase
{
    public LibraryEndpointTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Scan_NonexistentPath_Returns400()
    {
        await AuthorizeAsync();

        var response = await Client.PostAsync("/api/library/scan?path=/nonexistent/folder/xyz", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Scan_EmptyTempFolder_ReturnsZeroCounts()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"resonanse-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            await AuthorizeAsync();

            var response = await Client.PostAsync($"/api/library/scan?path={Uri.EscapeDataString(tempDir)}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ScanResult>();
            Assert.NotNull(result);
            Assert.Equal(0, result!.FilesProcessed);
            Assert.Equal(0, result.Errors);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_NoAuth_Returns401()
    {
        var response = await Client.PostAsync("/api/library/scan", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}