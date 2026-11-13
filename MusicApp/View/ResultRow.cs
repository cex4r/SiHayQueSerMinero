using MusicApp.Controller;
using MusicApp.Model;

namespace MusicApp.View;

/// <summary>Fila de lista para la vista: texto a mostrar más el registro original (con su id real).</summary>
public sealed record ResultRow(string Title, string Subtitle, object Source)
{
    public static ResultRow From(Song s)
    {
        var parts = new List<string> { "Song" };
        if (!string.IsNullOrWhiteSpace(s.Artist)) parts.Add(s.Artist);
        if (!string.IsNullOrWhiteSpace(s.Album) && s.AlbumId != 0) parts.Add(s.Album);
        return new ResultRow(s.Title, string.Join(" • ", parts), s);
    }

    public static ResultRow From(Artist a) => new(a.Name, $"Artist • {a.Type}", a);

    public static ResultRow From(Album a) =>
        new(a.Name, a.Year is > 0 ? $"Album • {a.Year}" : "Album", a);

    public static List<ResultRow> From(SearchResults r) => r.Type switch
    {
        SearchType.Song => r.Songs.Select(s => From(s)).ToList(),
        SearchType.Artist => r.Artists.Select(a => From(a)).ToList(),
        _ => r.Albums.Select(a => From(a)).ToList()
    };
}
