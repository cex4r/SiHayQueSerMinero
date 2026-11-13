using System.Globalization;
using MusicApp.Model;

namespace MusicApp.Controller;

// Texto crudo de los formularios; LibraryController lo valida y convierte a modelos.
public sealed record ArtistInput(string Name, ArtistType Type, string StageName, string RealName, string BirthDate, string DeathDate);
public sealed record SongInput(string Title, string Artist, string Album, string Path, string Genre, string Track, string Year);
public sealed record AlbumInput(string Name, string Path, string Year);

public sealed class LibraryController(AppConfig config)
{
    // ---- Artistas ----

    public Result<int> AddArtist(ArtistInput input)
    {
        var built = BuildArtist(0, input);
        if (!built.Success) return Result<int>.FailFrom(built);
        return DbAccess.Run(config.DbPath, c => LibraryEditor.AddArtist(built.Value!, c));
    }

    public Result<int> UpdateArtist(int id, ArtistInput input)
    {
        var built = BuildArtist(id, input);
        if (!built.Success) return Result<int>.FailFrom(built);
        return DbAccess.Run(config.DbPath, c => { LibraryEditor.UpdateArtist(built.Value!, c); return id; });
    }

    // ---- Canciones ----

    public Result<int> AddSong(SongInput input)
    {
        var built = BuildSong(0, input);
        if (!built.Success) return Result<int>.FailFrom(built);
        return DbAccess.Run(config.DbPath, c => LibraryEditor.AddSong(built.Value!, c));
    }

    public Result<int> UpdateSong(int id, SongInput input)
    {
        var built = BuildSong(id, input);
        if (!built.Success) return Result<int>.FailFrom(built);
        return DbAccess.Run(config.DbPath, c => { LibraryEditor.UpdateSong(built.Value!, c); return id; });
    }

    // ---- Álbumes ----

    public Result<int> AddAlbum(AlbumInput input)
    {
        var built = BuildAlbum(0, input);
        if (!built.Success) return Result<int>.FailFrom(built);
        return DbAccess.Run(config.DbPath, c => LibraryEditor.AddAlbum(built.Value!, c));
    }

    public Result<int> UpdateAlbum(int id, AlbumInput input)
    {
        if (id == 0)
            return Result<int>.Fail(ErrorKind.Validation, "El álbum \"Unknown\" es un valor reservado y no se puede editar.");
        var built = BuildAlbum(id, input);
        if (!built.Success) return Result<int>.FailFrom(built);
        return DbAccess.Run(config.DbPath, c => { LibraryEditor.UpdateAlbum(built.Value!, c); return id; });
    }

    // ---- Validación ----

    private static Result<Artist> BuildArtist(int id, ArtistInput i)
    {
        var name = i.Name.Trim();
        if (name.Length == 0) return Invalid<Artist>("El nombre artístico es obligatorio.");

        var artist = new Artist { Id = id, Name = name, Type = i.Type };
        if (i.Type != ArtistType.Person) return Result<Artist>.Ok(artist);

        if (!IsValidDateOrEmpty(i.BirthDate)) return Invalid<Artist>("Fecha de nacimiento no válida. Usa aaaa-mm-dd o solo el año.");
        if (!IsValidDateOrEmpty(i.DeathDate)) return Invalid<Artist>("Fecha de fallecimiento no válida. Usa aaaa-mm-dd o solo el año.");

        artist.Person = new PersonInfo
        {
            StageName = NullIfBlank(i.StageName) ?? name,
            RealName = NullIfBlank(i.RealName),
            BirthDate = NullIfBlank(i.BirthDate),
            DeathDate = NullIfBlank(i.DeathDate)
        };
        return Result<Artist>.Ok(artist);
    }

    private static Result<Song> BuildSong(int id, SongInput i)
    {
        if (string.IsNullOrWhiteSpace(i.Title)) return Invalid<Song>("El título es obligatorio.");
        if (string.IsNullOrWhiteSpace(i.Artist)) return Invalid<Song>("El intérprete es obligatorio.");
        if (string.IsNullOrWhiteSpace(i.Path)) return Invalid<Song>("La ruta del archivo MP3 es obligatoria.");
        if (!TryParseOptionalInt(i.Track, 0, 9999, out var track)) return Invalid<Song>("La pista debe ser un número entero positivo.");
        if (!TryParseOptionalInt(i.Year, 0, 9999, out var year)) return Invalid<Song>("El año debe ser un número entre 0 y 9999.");

        return Result<Song>.Ok(new Song
        {
            Id = id,
            Title = i.Title.Trim(),
            Artist = i.Artist.Trim(),
            Album = NullIfBlank(i.Album),
            Path = i.Path.Trim(),
            Genre = NullIfBlank(i.Genre),
            Track = track,
            Year = year
        });
    }

    private static Result<Album> BuildAlbum(int id, AlbumInput i)
    {
        if (string.IsNullOrWhiteSpace(i.Name)) return Invalid<Album>("El nombre del álbum es obligatorio.");
        if (!TryParseOptionalInt(i.Year, 0, 9999, out var year)) return Invalid<Album>("El año debe ser un número entre 0 y 9999.");

        return Result<Album>.Ok(new Album { Id = id, Name = i.Name.Trim(), Path = NullIfBlank(i.Path), Year = year });
    }

    private static Result<T> Invalid<T>(string message) => Result<T>.Fail(ErrorKind.Validation, message);

    private static string? NullIfBlank(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static bool TryParseOptionalInt(string text, int min, int max, out int? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text)) return true;
        if (!int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var n) || n < min || n > max)
            return false;
        value = n;
        return true;
    }

    private static bool IsValidDateOrEmpty(string text) =>
        string.IsNullOrWhiteSpace(text) ||
        DateTime.TryParseExact(text.Trim(), new[] { "yyyy-MM-dd", "yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
}
