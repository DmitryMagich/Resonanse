using Resonanse.Application.Metadata;

namespace Resonanse.Application.Abstractions;

public interface IMetadataReader
{
    AudioMetadata? Read(string filePath);
}