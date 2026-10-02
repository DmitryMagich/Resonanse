using System.Net;
using System.Net.Http.Json;
using Resonanse.Application.Dtos.Auth;

namespace Resonanse.IntegrationTests;

[Collection("Integration")]
public class AuthAndHealthTests : IntegrationTestBase
{
    public AuthAndHealthTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Health_NoAuth_Returns200()
    {
        var response = await Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Tracks_NoAuth_Returns401()
    {
        ClearAuthorization();

        var response = await Client.GetAsync("/api/tracks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = CustomWebApplicationFactory.TestAdminUsername,
            Password = "definitely-wrong"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = CustomWebApplicationFactory.TestAdminUsername,
            Password = CustomWebApplicationFactory.TestAdminPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrEmpty(auth!.AccessToken));
        Assert.False(string.IsNullOrEmpty(auth.RefreshToken));
        Assert.Equal(CustomWebApplicationFactory.TestAdminUsername, auth.User.Username);
    }

    [Fact]
    public async Task Tracks_ValidToken_Returns200()
    {
        await AuthorizeAsync();

        var response = await Client.GetAsync("/api/tracks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task NonAdmin_CannotCreateInvite_Returns403()
    {
        // Логинимся админом, создаём пользователя, получаем его токен
        var userToken = await CreateUserAndGetTokenAsync("regular-user-1", "password123");

        // Подменяем токен на пользовательский
        SetRawToken(userToken);

        var response = await Client.PostAsJsonAsync("/api/admin/invites", new { validDays = 1 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CreateInvite_ThenRegister_Succeeds()
    {
        await AuthorizeAsync();

        // Создаём инвайт
        var inviteResp = await Client.PostAsJsonAsync("/api/admin/invites", new { validDays = 1 });
        Assert.Equal(HttpStatusCode.OK, inviteResp.StatusCode);
        var invite = await inviteResp.Content.ReadFromJsonAsync<InviteDto>();
        Assert.NotNull(invite);
        Assert.False(string.IsNullOrEmpty(invite!.Code));

        // Регистрируемся
        var uniqueName = $"user-{Guid.NewGuid():N}"[..20];
        var regResp = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            InviteCode = invite.Code,
            Username = uniqueName,
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);
        var auth = await regResp.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.Equal(uniqueName, auth!.User.Username);
        Assert.Equal(0, (int)auth.User.Role); // User role
    }

    [Fact]
    public async Task Register_InvalidInvite_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            InviteCode = "not-a-real-invite",
            Username = "someone",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        await AuthorizeAsync();
        var inviteResp = await Client.PostAsJsonAsync("/api/admin/invites", new { validDays = 1 });
        var invite = await inviteResp.Content.ReadFromJsonAsync<InviteDto>();

        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            InviteCode = invite!.Code,
            Username = "validname",
            Password = "123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}