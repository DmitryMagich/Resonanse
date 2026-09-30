namespace Resonanse.Application.Dtos.Auth;

public class RegisterRequest
{
    public string InviteCode { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}