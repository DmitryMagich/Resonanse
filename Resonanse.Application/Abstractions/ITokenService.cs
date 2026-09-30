using Resonanse.Domain.Entities;

namespace Resonanse.Application.Abstractions;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}