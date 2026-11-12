using Microsoft.Data.Sqlite;

namespace MusicApp.Model;

public sealed class DuplicateRecordException(string message) : Exception(message);
public sealed class RecordNotFoundException(string message) : Exception(message);

/// <summary>
/// Escrituras en SQLite. Cada método abre su propia transacción sobre la conexión recibida;
/// si algo lanza excepción antes del Commit, la transacción se revierte al liberarse.
/// </summary>
public static class LibraryEditor
{
    public static int AddArtist(Artist artist, SqliteConnection connection)
    {
        using var tx = connection.BeginTransaction();
        if (PerformerExists(connection, tx, artist.Name, artist.Type, null))
            throw new DuplicateRecordException($"Ya existe el intérprete \"{artist.Name}\" con ese tipo.");

        var id = Convert.ToInt32(Scalar(connection, tx,
            "INSERT INTO performers (id_type, name) VALUES (@t, @n) RETURNING id_performer",
            ("@t", (int)artist.Type), ("@n", artist.Name)));

        if (artist.Type == ArtistType.Person) UpsertPerson(connection, tx, id, artist);
        tx.Commit();
        return id;
    }

    public static int AddAlbum(Album album, SqliteConnection connection)
    {
        using var tx = connection.BeginTransaction();
        if (AlbumExists(connection, tx, album.Name, album.Year, null))
            throw new DuplicateRecordException($"Ya existe el álbum \"{album.Name}\" con ese año.");

        var id = Convert.ToInt32(Scalar(connection, tx,
            "INSERT INTO albums (path, name, year) VALUES (@p, @n, @y) RETURNING id_album",
            ("@p", album.Path), ("@n", album.Name), ("@y", album.Year)));
        tx.Commit();
        return id;
    }

    /// <summary>
    /// song.Artist y song.Album vienen como texto del formulario: se reutiliza el registro existente
    /// con ese nombre o se crea uno nuevo (intérprete Unknown; álbum vacío = álbum 0).
    /// </summary>
    public static int AddSong(Song song, SqliteConnection connection)
    {
        using var tx = connection.BeginTransaction();
        if (Scalar(connection, tx, "SELECT 1 FROM rolas WHERE path = @p LIMIT 1", ("@p", song.Path)) is not null)
            throw new DuplicateRecordException("Ya existe una canción con esa ruta de archivo.");

        var performerId = ResolvePerformer(connection, tx, song.Artist!);
        var albumId = ResolveAlbum(connection, tx, song.Album, song.Year);

        var id = Convert.ToInt32(Scalar(connection, tx,
            "INSERT INTO rolas (id_performer, id_album, path, title, track, year, genre) " +
            "VALUES (@pf, @al, @p, @t, @tr, @y, @g) RETURNING id_rola",
            ("@pf", performerId), ("@al", albumId), ("@p", song.Path), ("@t", song.Title),
            ("@tr", song.Track), ("@y", song.Year), ("@g", song.Genre)));
        tx.Commit();
        return id;
    }

    public static void UpdateArtist(Artist artist, SqliteConnection connection)
    {
        using var tx = connection.BeginTransaction();
        if (PerformerExists(connection, tx, artist.Name, artist.Type, artist.Id))
            throw new DuplicateRecordException($"Ya existe el intérprete \"{artist.Name}\" con ese tipo.");

        var rows = Exec(connection, tx,
            "UPDATE performers SET id_type = @t, name = @n WHERE id_performer = @id",
            ("@t", (int)artist.Type), ("@n", artist.Name), ("@id", artist.Id));
        if (rows == 0) throw new RecordNotFoundException("El intérprete ya no existe en la base de datos.");

        // Si deja de ser Person no se borra la fila de persons para no perder datos.
        if (artist.Type == ArtistType.Person) UpsertPerson(connection, tx, artist.Id, artist);
        tx.Commit();
    }

    public static void UpdateAlbum(Album album, SqliteConnection connection)
    {
        using var tx = connection.BeginTransaction();
        if (AlbumExists(connection, tx, album.Name, album.Year, album.Id))
            throw new DuplicateRecordException($"Ya existe el álbum \"{album.Name}\" con ese año.");

        var rows = Exec(connection, tx,
            "UPDATE albums SET path = @p, name = @n, year = @y WHERE id_album = @id",
            ("@p", album.Path), ("@n", album.Name), ("@y", album.Year), ("@id", album.Id));
        if (rows == 0) throw new RecordNotFoundException("El álbum ya no existe en la base de datos.");
        tx.Commit();
    }

    public static void UpdateSong(Song song, SqliteConnection connection)
    {
        using var tx = connection.BeginTransaction();
        if (Scalar(connection, tx, "SELECT 1 FROM rolas WHERE path = @p AND id_rola <> @id LIMIT 1",
                ("@p", song.Path), ("@id", song.Id)) is not null)
            throw new DuplicateRecordException("Ya existe otra canción con esa ruta de archivo.");

        var performerId = ResolvePerformer(connection, tx, song.Artist!);
        var albumId = ResolveAlbum(connection, tx, song.Album, song.Year);

        var rows = Exec(connection, tx,
            "UPDATE rolas SET id_performer = @pf, id_album = @al, path = @p, title = @t, " +
            "track = @tr, year = @y, genre = @g WHERE id_rola = @id",
            ("@pf", performerId), ("@al", albumId), ("@p", song.Path), ("@t", song.Title),
            ("@tr", song.Track), ("@y", song.Year), ("@g", song.Genre), ("@id", song.Id));
        if (rows == 0) throw new RecordNotFoundException("La canción ya no existe en la base de datos.");
        tx.Commit();
    }

    // ---- ayudantes ----

    private static int ResolvePerformer(SqliteConnection c, SqliteTransaction tx, string name)
    {
        var existing = Scalar(c, tx,
            "SELECT id_performer FROM performers WHERE name = @n COLLATE NOCASE ORDER BY id_performer LIMIT 1",
            ("@n", name));
        if (existing is not null) return Convert.ToInt32(existing);

        return Convert.ToInt32(Scalar(c, tx,
            "INSERT INTO performers (id_type, name) VALUES (2, @n) RETURNING id_performer", ("@n", name)));
    }

    private static int ResolveAlbum(SqliteConnection c, SqliteTransaction tx, string? name, int? year)
    {
        if (string.IsNullOrWhiteSpace(name)) return 0; // álbum "Unknown" del esquema

        var existing = Scalar(c, tx,
            "SELECT id_album FROM albums WHERE name = @n COLLATE NOCASE ORDER BY id_album LIMIT 1", ("@n", name));
        if (existing is not null) return Convert.ToInt32(existing);

        return Convert.ToInt32(Scalar(c, tx,
            "INSERT INTO albums (path, name, year) VALUES ('', @n, @y) RETURNING id_album",
            ("@n", name), ("@y", year)));
    }

    // id_person = id_performer: convención de la app, ver README.
    private static void UpsertPerson(SqliteConnection c, SqliteTransaction tx, int id, Artist artist)
    {
        var p = artist.Person ?? new PersonInfo();
        Exec(c, tx,
            "INSERT INTO persons (id_person, stage_name, real_name, birth_date, death_date) " +
            "VALUES (@id, @s, @r, @b, @d) " +
            "ON CONFLICT(id_person) DO UPDATE SET stage_name = excluded.stage_name, " +
            "real_name = excluded.real_name, birth_date = excluded.birth_date, death_date = excluded.death_date",
            ("@id", id), ("@s", p.StageName ?? artist.Name), ("@r", p.RealName),
            ("@b", p.BirthDate), ("@d", p.DeathDate));
    }

    private static bool PerformerExists(SqliteConnection c, SqliteTransaction tx, string name, ArtistType type, int? excludeId) =>
        Scalar(c, tx,
            "SELECT 1 FROM performers WHERE name = @n COLLATE NOCASE AND id_type = @t " +
            "AND (@x IS NULL OR id_performer <> @x) LIMIT 1",
            ("@n", name), ("@t", (int)type), ("@x", excludeId)) is not null;

    private static bool AlbumExists(SqliteConnection c, SqliteTransaction tx, string name, int? year, int? excludeId) =>
        Scalar(c, tx,
            "SELECT 1 FROM albums WHERE name = @n COLLATE NOCASE AND IFNULL(year, 0) = IFNULL(@y, 0) " +
            "AND (@x IS NULL OR id_album <> @x) LIMIT 1",
            ("@n", name), ("@y", year), ("@x", excludeId)) is not null;

    private static SqliteCommand Command(SqliteConnection c, SqliteTransaction tx, string sql, (string Name, object? Value)[] ps)
    {
        var cmd = c.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        foreach (var (name, value) in ps) cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return cmd;
    }

    private static int Exec(SqliteConnection c, SqliteTransaction tx, string sql, params (string Name, object? Value)[] ps)
    {
        using var cmd = Command(c, tx, sql, ps);
        return cmd.ExecuteNonQuery();
    }

    private static object? Scalar(SqliteConnection c, SqliteTransaction tx, string sql, params (string Name, object? Value)[] ps)
    {
        using var cmd = Command(c, tx, sql, ps);
        return cmd.ExecuteScalar();
    }
}
