using Microsoft.Extensions.Configuration;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Metadata;

namespace Resonanse.Infrastructure.Covers;

public class FileSystemCoverStorage : ICoverStorage
{
    private readonly string _basePath;

    public FileSystemCoverStorage(IConfiguration configuration)
    {
        var configured = configuration["Resonanse:CoversPath"];
        _basePath = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(AppContext.BaseDirectory, "covers")
            : configured;

        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(Guid albumId, CoverMetadata data, CancellationToken ct = default)
    {
        var ext = data.MimeType switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };

        var fileName = $"{albumId}{ext}";
        var fullPath = Path.Combine(_basePath, fileName);

        // Удаляем старые версии с другим расширением
        foreach (var oldExt in new[] { ".jpg", ".png", ".webp" })
        {
            if (oldExt == ext) continue;
            var oldPath = Path.Combine(_basePath, $"{albumId}{oldExt}");
            if (File.Exists(oldPath))
            {
                try { File.Delete(oldPath); } catch { /* ignore */ }
            }
        }

        await File.WriteAllBytesAsync(fullPath, data.Bytes, ct);

        return $"covers/{fileName}";
    }

    public string GetFullPath(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath);
        return Path.Combine(_basePath, fileName);
    }

    public bool Exists(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return false;
        return File.Exists(GetFullPath(relativePath));
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var path = GetFullPath(relativePath);
        if (File.Exists(path))
        {
            try { File.Delete(path); } catch { /* ignore */ }
        }
        return Task.CompletedTask;
    }
}