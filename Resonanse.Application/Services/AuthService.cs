using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos.Auth;
using Resonanse.Domain.Entities;
using Resonanse.Domain.Enums;

namespace Resonanse.Application.Services;

public class AuthService : IAuthService
{
    private readonly IResonanseDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(IResonanseDbContext db, IPasswordHasher hasher, ITokenService tokens)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        ValidateUsername(request.Username);
        ValidatePassword(request.Password);

        var code = request.InviteCode?.Trim();
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Invite code is required.");

        var invite = await _db.Invites
            .Include(i => i.UsedBy)
            .FirstOrDefaultAsync(i => i.Code == code, ct)
            ?? throw new InvalidOperationException("Invite not found.");

        if (!invite.IsActive)
            throw new InvalidOperationException("Invite is expired or already used.");

        var username = request.Username.Trim();
        var exists = await _db.Users.AnyAsync(u => u.Username == username, ct);
        if (exists)
            throw new InvalidOperationException("Username is already taken.");

        var user = new User
        {
            Username = username,
            PasswordHash = _hasher.Hash(request.Password),
            Role = UserRole.User
        };
        _db.Users.Add(user);

        invite.UsedByUserId = user.Id;
        invite.UsedAt = DateTime.UtcNow;
        invite.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var username = request.Username?.Trim() ?? string.Empty;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Invalid username or password.");

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (stored is null || !stored.IsActive)
            throw new InvalidOperationException("Invalid or expired refresh token.");

        stored.RevokedAt = DateTime.UtcNow;
        stored.UpdatedAt = DateTime.UtcNow;

        return await IssueTokensAsync(stored.User, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (stored is not null && stored.RevokedAt is null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            stored.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        var (accessToken, accessExpires) = _tokens.GenerateAccessToken(user);
        var refreshToken = _tokens.GenerateRefreshToken();

        var entity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        _db.RefreshTokens.Add(entity);

        await _db.SaveChangesAsync(ct);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessExpires,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            }
        };
    }

    private static void ValidateUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Trim().Length < 3)
            throw new InvalidOperationException("Username must be at least 3 characters.");
    }

    private static void ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new InvalidOperationException("Password must be at least 6 characters.");
    }
}