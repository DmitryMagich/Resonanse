namespace Resonanse.Application.Dtos;

public class CreateInviteRequestBody
{
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string? Contact { get; set; }
}