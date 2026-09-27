namespace Resonanse.Application.Abstractions;

public interface IFileHasher
{
    Task<string> ComputeHashAsync(string filePath, CancellationToken cancellationToken = default);
}