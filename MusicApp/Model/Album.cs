namespace MusicApp.Model;

/// <summary>Álbum (tabla albums). El id 0 es el álbum "Unknown" del esquema.</summary>
public sealed class Album
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Path { get; set; }
    public int? Year { get; set; }
}
