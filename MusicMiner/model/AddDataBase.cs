
using Microsoft.Data.Sqlite;

class AddDataBase
{
    public long GetOrCreateSong(
        Song song,
        string dataBasePath,
        string albumPath)
    {
        using var connection = new SqliteConnection(
            $"Data Source={dataBasePath}"
        );

        connection.Open();

        using var transaction = connection.BeginTransaction();

        // Primero verificamos si la canción ya existe.
        using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
            SELECT id_rola FROM rolas
            WHERE path = $path
            LIMIT 1;
            """;

        command.Parameters.AddWithValue("$path", song.Path);

        object? result = command.ExecuteScalar();

        if (result is long existingId)
        {
            transaction.Commit();
            return existingId;
        }

        // Obtenemos los identificadores necesarios.
        string artist = song.Artist.FirstOrDefault()
            ?? "Unknown";

        long performerId = GetOrCreatePerformer(
            artist, connection, transaction
        );

        long albumId = GetOrCreateAlbum(
            song.Album,
            albumPath,
            song.Year,
            connection,
            transaction
        );

        // Insertamos la canción.
        command.Parameters.Clear();

        command.CommandText = """
            INSERT INTO rolas
                (id_performer, id_album, path,
                 title, track, year, genre)
            VALUES
                ($performer, $album, $path,
                 $title, $track, $year, $genre);

            SELECT last_insert_rowid();
            """;

        command.Parameters.AddWithValue(
            "$performer", performerId
        );

        command.Parameters.AddWithValue(
            "$album", albumId
        );

        command.Parameters.AddWithValue(
            "$path", song.Path
        );

        command.Parameters.AddWithValue(
            "$title", (object?)song.Title ?? DBNull.Value
        );

        command.Parameters.AddWithValue(
            "$track",
            song.Track.HasValue
                ? (object)(long)song.Track.Value
                : DBNull.Value
        );

        command.Parameters.AddWithValue(
            "$year",
            song.Year.HasValue
                ? (object)(long)song.Year.Value
                : DBNull.Value
        );

        command.Parameters.AddWithValue(
            "$genre",
            (object?)song.Genre.FirstOrDefault()
                ?? DBNull.Value
        );

        long songId = (long)command.ExecuteScalar()!;

        transaction.Commit();

        return songId;
    }

    private long GetOrCreatePerformer(
        string artistSong,
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
            SELECT id_performer FROM performers
            WHERE name = $name
            LIMIT 1;
            """;

        command.Parameters.AddWithValue(
            "$name", artistSong
        );

        object? result = command.ExecuteScalar();

        if (result is long performerId)
            return performerId;

        command.Parameters.Clear();

        command.CommandText = """
            INSERT INTO performers (id_type, name)
            VALUES ($id_type, $name);

            SELECT last_insert_rowid();
            """;

        command.Parameters.AddWithValue("$id_type", 2);
        command.Parameters.AddWithValue("$name", artistSong);

        return (long)command.ExecuteScalar()!;
    }

    private long GetOrCreateAlbum(
        string albumName,
        string albumPath,
        uint? year,
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
            SELECT id_album FROM albums
            WHERE path = $path AND name = $name
            LIMIT 1;
            """;

        command.Parameters.AddWithValue(
            "$path", albumPath
        );

        command.Parameters.AddWithValue(
            "$name", albumName
        );

        object? result = command.ExecuteScalar();

        if (result is long albumId)
            return albumId;

        command.Parameters.Clear();

        command.CommandText = """
            INSERT INTO albums (path, name, year)
            VALUES ($path, $name, $year);

            SELECT last_insert_rowid();
            """;

        command.Parameters.AddWithValue(
            "$path", albumPath
        );

        command.Parameters.AddWithValue(
            "$name", albumName
        );

        command.Parameters.AddWithValue(
            "$year",
            year.HasValue
                ? (object)(long)year.Value
                : DBNull.Value
        );

        return (long)command.ExecuteScalar()!;
    }
}
