using Resonanse.Domain.Enums;

namespace Resonanse.Application.Dtos.Auth;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}