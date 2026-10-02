using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Resonanse.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Resonanse.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestUsername = "testuser";
    public const string TestPassword = "testpassword";
    public const string TestAdminUsername = "admin";
    public const string TestAdminPassword = "test-admin-password";
    public const string TestJwtSigningKey = "test-signing-key-at-least-32-characters-long-xyz-12345";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("resonanse_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();
        // Program.cs при старте сразу дёргает PeerSeeder, которому нужны таблицы.
        var options = new DbContextOptionsBuilder<ResonanseDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        await using var db = new ResonanseDbContext(options);
        await db.Database.MigrateAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ResonanseDb"] = _dbContainer.GetConnectionString(),
                ["Resonanse:Admin:Username"] = TestAdminUsername,
                ["Resonanse:Admin:Password"] = TestAdminPassword,
                ["Resonanse:Jwt:Issuer"] = "Resonanse",
                ["Resonanse:Jwt:Audience"] = "Resonanse.Clients",
                ["Resonanse:Jwt:SigningKey"] = TestJwtSigningKey,
                ["Resonanse:Jwt:AccessTokenMinutes"] = "60",
                ["Resonanse:AutoScan:Enabled"] = "false",
                ["Resonanse:PeerName"] = "integration-test-peer",
                ["Resonanse:ExcludedFolders:0"] = "AutoEq-master",
                ["Resonanse:ExcludedFolders:1"] = "HeSuVi-HRIRs",
            });
        });
    }
}