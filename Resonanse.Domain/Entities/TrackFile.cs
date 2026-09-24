using Resonanse.Domain.Enums;

namespace Resonanse.Domain.Entities;

/// <summary>
/// Физический файл трека на конкретном узле.
/// Один Track может иметь много TrackFile (разные форматы, качества, узлы).
/// </summary>
public class TrackFile : BaseEntity
{
    // Связь с логическим треком
    public Guid TrackId { get; set; }
    public Track Track { get; set; } = null!;

    // Связь с узлом-владельцем
    public Guid PeerId { get; set; }
    public Peer Peer { get; set; } = null!;
    // Путь на диске узла
    public string FilePath { get; set; } = string.Empty;
    /// <summary>
    /// BLAKE3-хэш файла (hex-строка). Основной идентификатор файла в P2P-сети.
    /// </summary>
    public string FileHash { get; set; } = string.Empty;

    public long FileSize { get; set; } // в байтах

    // Аудио-характеристики
    public AudioFormat Format { get; set; } = AudioFormat.Unknown;
    public int Bitrate { get; set; }     // kbps
    public int SampleRate { get; set; }  // Hz
    public int? BitDepth { get; set; }   // 16/24/32 для FLAC/WAV, null для MP3
    public int Channels { get; set; }    // 1 = mono, 2 = stereo
}