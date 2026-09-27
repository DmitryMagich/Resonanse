namespace Resonanse.Api.Auth;

public class BasicAuthOptions
{
    public const string SectionName = "Resonanse:Auth";

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}