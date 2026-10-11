using System.Threading.Channels;

namespace Resonanse.Application.Scanning;

/// <summary>
/// In-process queue for background scan jobs.
/// Uses bounded channel to prevent unbounded memory growth under load.
/// </summary>
public class ScanQueue
{
    private readonly Channel<ScanRequest> _channel;

    public ScanQueue()
    {
        _channel = Channel.CreateBounded<ScanRequest>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public bool TryEnqueue(ScanRequest request) => _channel.Writer.TryWrite(request);

    public IAsyncEnumerable<ScanRequest> ReadAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}