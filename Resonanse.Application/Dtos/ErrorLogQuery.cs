namespace Resonanse.Application.Dtos;

public class ErrorLogQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Level { get; set; }      // Warning, Error, Critical
    public string? Search { get; set; }     // Message/Path
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? UserId { get; set; }
}