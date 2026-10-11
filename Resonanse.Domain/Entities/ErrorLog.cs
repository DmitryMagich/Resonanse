namespace Resonanse.Domain.Entities;

public class ErrorLog : BaseEntity
{
    public string Level { get; set; } = "Error";
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? ExceptionType { get; set; }
    public string? Path { get; set; }
    public string? Method { get; set; }
    public int? StatusCode { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? QueryString { get; set; }
    public string? Category { get; set; }
    public string? AdditionalData { get; set; }
}