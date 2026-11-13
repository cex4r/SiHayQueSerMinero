using MusicApp.Model;

namespace MusicApp.Controller;

public enum SearchType
{
    Song,
    Artist,
    Album
}

/// <summary>Respuesta tipada: solo la colección que corresponde a Type tiene elementos.</summary>
public sealed class SearchResults
{
    public SearchType Type { get; private init; }
    public IReadOnlyList<Song> Songs { get; private init; } = [];
    public IReadOnlyList<Artist> Artists { get; private init; } = [];
    public IReadOnlyList<Album> Albums { get; private init; } = [];

    public int Count => Type switch
    {
        SearchType.Song => Songs.Count,
        SearchType.Artist => Artists.Count,
        _ => Albums.Count
    };

    public static SearchResults FromSongs(List<Song> songs) => new() { Type = SearchType.Song, Songs = songs };
    public static SearchResults FromArtists(List<Artist> artists) => new() { Type = SearchType.Artist, Artists = artists };
    public static SearchResults FromAlbums(List<Album> albums) => new() { Type = SearchType.Album, Albums = albums };
}

public sealed class Finder(AppConfig config)
{
    /// <summary>Una sola conexión por búsqueda. Sin coincidencias: éxito con Count == 0.</summary>
    public Result<SearchResults> FindGlobal(string searchText, SearchType searchType)
    {
        var text = searchText.Trim();
        if (text.Length == 0)
            return Result<SearchResults>.Fail(ErrorKind.Validation, "Escribe algo para buscar.");

        return DbAccess.Run(config.DbPath, connection => searchType switch
        {
            SearchType.Song => SearchResults.FromSongs(Find.FindSong(text, connection)),
            SearchType.Artist => SearchResults.FromArtists(Find.FindArtist(text, connection)),
            SearchType.Album => SearchResults.FromAlbums(Find.FindAlbum(text, connection)),
            _ => throw new ArgumentOutOfRangeException(nameof(searchType))
        });
    }

    /// <summary>Todas las canciones registradas (LibraryView).</summary>
    public Result<List<Song>> ListSongs() => DbAccess.Run(config.DbPath, Find.ListSongs);
}
