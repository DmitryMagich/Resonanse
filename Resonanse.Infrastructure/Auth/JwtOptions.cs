namespace Resonanse.Infrastructure.Auth;

public class JwtOptions
{
    public const string SectionName = "Resonanse:Jwt";

    public string Issuer { get; set; } = "Resonanse";
    public string Audience { get; set; } = "Resonanse.Clients";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
}