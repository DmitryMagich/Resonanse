using System.Threading.Channels;
using Resonanse.Application.Dtos;

namespace Resonanse.Infrastructure.Logging;

public class ErrorLogChannel
{
    private readonly Channel<ErrorLogDto> _channel;

    public ErrorLogChannel()
    {
        _channel = Channel.CreateBounded<ErrorLogDto>(new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public bool TryWrite(ErrorLogDto log) => _channel.Writer.TryWrite(log);

    public IAsyncEnumerable<ErrorLogDto> ReadAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}