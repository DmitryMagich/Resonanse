using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Resonanse.Application.Dtos.Auth;
using Resonanse.Infrastructure.Persistence;

namespace Resonanse.IntegrationTests;

[Collection("Integration")]
public abstract class IntegrationTestBase
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;

    private string? _accessToken;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    /// <summary>
    /// Логинится админом и ставит Bearer-токен на HttpClient.
    /// Идемпотентно — если токен уже есть, не логинится повторно.
    /// </summary>
    protected async Task AuthorizeAsync()
    {
        if (_accessToken is not null)
        {
            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
            return;
        }

        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = CustomWebApplicationFactory.TestAdminUsername,
            Password = CustomWebApplicationFactory.TestAdminPassword
        });

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Login failed: {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>()
            ?? throw new InvalidOperationException("Login returned empty body.");

        _accessToken = auth.AccessToken;
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    /// <summary>
    /// Логинится как новый пользователь (по инвайту) и подменяет токен на его.
    /// </summary>
    protected async Task<string> CreateUserAndGetTokenAsync(string username, string password)
    {
        // 1. Логин админа для создания инвайта
        await AuthorizeAsync();

        // 2. Создать инвайт
        var inviteResp = await Client.PostAsJsonAsync("/api/admin/invites", new { validDays = 1 });
        inviteResp.EnsureSuccessStatusCode();
        var invite = await inviteResp.Content.ReadFromJsonAsync<InviteDto>()
            ?? throw new InvalidOperationException("Invite creation failed.");

        // 3. Регистрация нового пользователя
        var regResp = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            InviteCode = invite.Code,
            Username = username,
            Password = password
        });
        regResp.EnsureSuccessStatusCode();
        var auth = await regResp.Content.ReadFromJsonAsync<AuthResponse>()
            ?? throw new InvalidOperationException("Register returned empty body.");

        return auth.AccessToken;
    }

    protected void ClearAuthorization()
    {
        _accessToken = null;
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected void SetRawToken(string token)
    {
        _accessToken = token;
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>Прямой доступ к БД.</summary>
    protected async Task<T> WithDbAsync<T>(Func<ResonanseDbContext, Task<T>> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResonanseDbContext>();
        return await action(db);
    }

    protected async Task WithDbAsync(Func<ResonanseDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResonanseDbContext>();
        await action(db);
    }
}