using System.Text.Json;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;
using Resonanse.Infrastructure.Logging;

namespace Resonanse.Api.Middleware;

public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionLoggingMiddleware> _logger;
    private readonly ErrorLogChannel _channel;

    public ExceptionLoggingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionLoggingMiddleware> logger,
        ErrorLogChannel channel)
    {
        _next = next;
        _logger = logger;
        _channel = channel;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteErrorLogAsync(context, currentUser, ex);
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    error = "Internal server error",
                    traceId = context.TraceIdentifier
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        }
    }

    private Task WriteErrorLogAsync(HttpContext context, ICurrentUser currentUser, Exception ex)
    {
        var req = context.Request;

        var dto = new ErrorLogDto
        {
            Id = Guid.NewGuid(),
            Level = "Error",
            Message = Truncate(ex.Message, 4000),
            StackTrace = ex.ToString(),
            ExceptionType = ex.GetType().FullName,
            Path = Truncate(req.Path.Value ?? string.Empty, 1000),
            Method = req.Method,
            StatusCode = 500,
            UserId = currentUser.UserId,
            Username = currentUser.Username,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Truncate(req.Headers.UserAgent.ToString(), 500),
            QueryString = Truncate(req.QueryString.Value ?? string.Empty, 2000),
            Category = "UnhandledException",
            CreatedAt = DateTime.UtcNow
        };

        _channel.TryWrite(dto);
        return Task.CompletedTask;
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max];
}