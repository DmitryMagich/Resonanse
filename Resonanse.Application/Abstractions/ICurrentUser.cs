using Resonanse.Domain.Enums;

namespace Resonanse.Application.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Username { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}