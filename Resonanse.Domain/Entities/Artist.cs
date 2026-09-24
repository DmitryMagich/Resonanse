namespace Resonanse.Domain.Entities;

public class Artist : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ImagePath { get; set; }
    public ICollection<Album> Albums { get; set; } = new List<Album>();
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}