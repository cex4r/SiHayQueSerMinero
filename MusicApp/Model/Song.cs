namespace MusicApp.Model;

/// <summary>Canción (tabla rolas) con el nombre del intérprete y del álbum ya resueltos.</summary>
public sealed class Song
{
    public int Id { get; set; }
    public string Title { get; set; } = "";

    // Los ids permiten editar por identificador real en lugar de por nombre.
    public int? ArtistId { get; set; }
    public string? Artist { get; set; }
    public int? AlbumId { get; set; }
    public string? Album { get; set; }

    public string? Path { get; set; }
    public int? Track { get; set; }
    public int? Year { get; set; }
    public string? Genre { get; set; }
}
