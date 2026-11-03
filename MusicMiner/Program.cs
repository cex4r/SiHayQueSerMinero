
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        string dbPath = Path.Combine(
            Path.GetTempPath(),
            $"music-test-{Guid.NewGuid()}.db"
        );

        try
        {
            CreateTestDatabase(dbPath);

            var database = new AddDataBase();

            var song1 = new Song(
                "Bohemian Rhapsody",
                new[] { "Queen" },
                1975,
                "A Night at the Opera",
                new[] { "Queen" },
                1,
                new[] { "Rock" },
                "/music/queen/bohemian.mp3"
            );

            var song2 = new Song(
                "Love of My Life",
                new[] { "Queen" },
                1975,
                "A Night at the Opera",
                new[] { "Queen" },
                9,
                new[] { "Rock" },
                "/music/queen/love.mp3"
            );

            string albumPath = "/music/queen";

            // Prueba 1: insertar una canción.
            long id1 = database.GetOrCreateSong(
                song1, dbPath, albumPath
            );

            Check(id1 > 0, "Inserción inicial");

            // Prueba 2: insertar nuevamente la misma canción.
            long repeatedId = database.GetOrCreateSong(
                song1, dbPath, albumPath
            );

            Check(id1 == repeatedId, "Evitar duplicados");

            // Prueba 3: insertar otra canción del mismo álbum.
            long id2 = database.GetOrCreateSong(
                song2, dbPath, albumPath
            );

            Check(id1 != id2, "Canciones diferentes");

            using var connection = new SqliteConnection(
                $"Data Source={dbPath}"
            );
            connection.Open();

            Check(
                Count(connection, "rolas") == 2,
                "Dos canciones registradas"
            );

            Check(
                Count(connection, "performers") == 1,
                "No duplicar intérpretes"
            );

            Check(
                Count(connection, "albums") == 1,
                "No duplicar álbumes"
            );
        }
        finally
        {
            // Se elimina la base de datos de pruebas.
            SqliteConnection.ClearAllPools();

            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    static void CreateTestDatabase(string path)
    {
        using var connection = new SqliteConnection(
            $"Data Source={path}"
        );
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE types (
                id_type INTEGER PRIMARY KEY,
                name TEXT
            );

            CREATE TABLE performers (
                id_performer INTEGER PRIMARY KEY,
                id_type INTEGER,
                name TEXT
            );

            CREATE TABLE albums (
                id_album INTEGER PRIMARY KEY,
                path TEXT,
                name TEXT,
                year INTEGER
            );

            CREATE TABLE rolas (
                id_rola INTEGER PRIMARY KEY,
                id_performer INTEGER,
                id_album INTEGER,
                path TEXT UNIQUE,
                title TEXT,
                track INTEGER,
                year INTEGER,
                genre TEXT
            );

            INSERT INTO types (id_type, name)
            VALUES (2, 'Unknown');
            """;

        command.ExecuteNonQuery();
    }

    static long Count(SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();

        // Los nombres utilizados aquí son constantes
        // controladas por el propio programa de pruebas.
        command.CommandText = $"SELECT COUNT(*) FROM {table}";

        return (long)command.ExecuteScalar()!;
    }

    static void Check(bool condition, string description)
    {
        if (!condition)
            throw new Exception($"FALLÓ: {description}");

        Console.WriteLine($"OK: {description}");
    }
}
