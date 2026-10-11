using Resonanse.Application.Metadata;

namespace Resonanse.Application.Abstractions;

public interface ICoverStorage
{
    Task<string> SaveAsync(Guid albumId, CoverMetadata data, CancellationToken ct = default);
    string GetFullPath(string relativePath);
    bool Exists(string relativePath);
    Task DeleteAsync(string relativePath, CancellationToken ct = default);
}