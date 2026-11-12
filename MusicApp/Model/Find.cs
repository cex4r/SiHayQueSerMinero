using Microsoft.Data.Sqlite;

namespace MusicApp.Model;

/// <summary>Consultas de solo lectura. Sin coincidencias devuelve lista vacía; los fallos técnicos se propagan.</summary>
public static class Find
{
    private const string SongSelect =
        "SELECT r.id_rola, r.title, r.id_performer, p.name, r.id_album, a.name, r.path, r.track, r.year, r.genre " +
        "FROM rolas r " +
        "LEFT JOIN performers p ON p.id_performer = r.id_performer " +
        "LEFT JOIN albums a ON a.id_album = r.id_album";

    public static List<Song> FindSong(string name, SqliteConnection connection) =>
        QuerySongs(connection, " WHERE r.title LIKE @q ESCAPE '\\'", name);

    /// <summary>Todas las canciones, para LibraryView.</summary>
    public static List<Song> ListSongs(SqliteConnection connection) => QuerySongs(connection, "", null);

    public static List<Artist> FindArtist(string name, SqliteConnection connection)
    {
        // persons se une por id_person = id_performer: el esquema no define otro vínculo
        // (ver README, "Decisiones sobre el esquema").
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "SELECT f.id_performer, f.name, f.id_type, pe.stage_name, pe.real_name, pe.birth_date, pe.death_date " +
            "FROM performers f " +
            "LEFT JOIN types t ON t.id_type = f.id_type " +
            "LEFT JOIN persons pe ON pe.id_person = f.id_performer AND f.id_type = 0 " +
            "WHERE f.name LIKE @q ESCAPE '\\' ORDER BY f.name COLLATE NOCASE";
        cmd.Parameters.AddWithValue("@q", Like(name));

        var list = new List<Artist>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var type = (ArtistType)(reader.GetIntOrNull(2) ?? (int)ArtistType.Unknown);
            var artist = new Artist
            {
                Id = reader.GetInt32(0),
                Name = reader.GetStringOrNull(1) ?? "",
                Type = type
            };
            if (type == ArtistType.Person)
            {
                artist.Person = new PersonInfo
                {
                    StageName = reader.GetStringOrNull(3),
                    RealName = reader.GetStringOrNull(4),
                    BirthDate = reader.GetStringOrNull(5),
                    DeathDate = reader.GetStringOrNull(6)
                };
            }
            list.Add(artist);
        }
        return list;
    }

    public static List<Album> FindAlbum(string name, SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "SELECT id_album, name, path, year FROM albums " +
            "WHERE name LIKE @q ESCAPE '\\' ORDER BY name COLLATE NOCASE";
        cmd.Parameters.AddWithValue("@q", Like(name));

        var list = new List<Album>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Album
            {
                Id = reader.GetInt32(0),
                Name = reader.GetStringOrNull(1) ?? "",
                Path = reader.GetStringOrNull(2),
                Year = reader.GetIntOrNull(3)
            });
        }
        return list;
    }

    private static List<Song> QuerySongs(SqliteConnection connection, string where, string? name)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = SongSelect + where + " ORDER BY r.title COLLATE NOCASE";
        if (name is not null) cmd.Parameters.AddWithValue("@q", Like(name));

        var list = new List<Song>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Song
            {
                Id = reader.GetInt32(0),
                Title = reader.GetStringOrNull(1) ?? "",
                ArtistId = reader.GetIntOrNull(2),
                Artist = reader.GetStringOrNull(3),
                AlbumId = reader.GetIntOrNull(4),
                Album = reader.GetStringOrNull(5),
                Path = reader.GetStringOrNull(6),
                Track = reader.GetIntOrNull(7),
                Year = reader.GetIntOrNull(8),
                Genre = reader.GetStringOrNull(9)
            });
        }
        return list;
    }

    // Escapa % y _ para que el texto del usuario se busque literalmente.
    private static string Like(string text) =>
        "%" + text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
}
