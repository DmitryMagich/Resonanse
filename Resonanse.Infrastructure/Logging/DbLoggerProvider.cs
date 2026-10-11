using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Resonanse.Application.Dtos;

namespace Resonanse.Infrastructure.Logging;

public class DbLoggerProvider : ILoggerProvider
{
    private readonly ErrorLogChannel _channel;
    private readonly LogLevel _minLevel;
    private readonly HashSet<string> _excludedPrefixes;

    public DbLoggerProvider(
        ErrorLogChannel channel,
        LogLevel minLevel,
        IEnumerable<string> excludedPrefixes)
    {
        _channel = channel;
        _minLevel = minLevel;
        _excludedPrefixes = new HashSet<string>(excludedPrefixes, StringComparer.OrdinalIgnoreCase);
    }

    public ILogger CreateLogger(string categoryName)
    {
        foreach (var prefix in _excludedPrefixes)
        {
            if (categoryName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return NullLogger.Instance;
        }

        return new DbLogger(categoryName, _minLevel, _channel);
    }

    public void Dispose() { }
}

internal class DbLogger : ILogger
{
    private readonly string _category;
    private readonly LogLevel _minLevel;
    private readonly ErrorLogChannel _channel;

    public DbLogger(string category, LogLevel minLevel, ErrorLogChannel channel)
    {
        _category = category;
        _minLevel = minLevel;
        _channel = channel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message) && exception is null) return;

        var dto = new ErrorLogDto
        {
            Id = Guid.NewGuid(),
            Level = logLevel.ToString(),
            Message = Truncate(message ?? exception!.Message, 4000),
            StackTrace = exception?.ToString(),
            ExceptionType = exception?.GetType().FullName,
            Category = _category,
            CreatedAt = DateTime.UtcNow
        };

        _channel.TryWrite(dto);
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max];
}