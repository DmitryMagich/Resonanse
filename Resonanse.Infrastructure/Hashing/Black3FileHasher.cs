using Blake3;
using Resonanse.Application.Abstractions;

namespace Resonanse.Infrastructure.Hashing;

public class Blake3FileHasher : IFileHasher
{
    public async Task<string> ComputeHashAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(filePath);
        using var hasher = Hasher.New();

        var buffer = new byte[81920];
        int read;
        while ((read = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            hasher.Update(buffer.AsSpan(0, read));
        }

        return hasher.Finalize().ToString();
    }
}