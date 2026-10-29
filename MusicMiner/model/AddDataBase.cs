using Microsoft.Data.Sqlite;

class AddDataBase
{
    public long GetOrCreateSong(Song song, string dataBasePath, string albumPath)
    {  
        
        using var connection = new SqliteConnection($"Data Source={dataBasePath}");
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
        SELECT id_rola FROM rolas
        WHERE path = $path
        LIMIT 1;
        """;

        command.Parameters.AddWithValue("$path", song.Path);

        object? result = command.ExecuteScalar();
        command.Parameters.Clear();

        if(result == null)
        {
            //El performer no se encuentra en la base de datos.
            //Los creamos y los insertamos.
            command.CommandText = """
            INSERT INTO rolas (id_performer, id_album, path, title, track, year, genre)
            VALUES ($performer, $album, $path, $title, $track, $year, $genre);
            """;

            long performerId = GetOrCreatePerformer(song.Artist[0],dataBasePath);

            long albumId = GetOrCreateAlbum(song.Album,dataBasePath,albumPath,(int)song.Year);

            command.Parameters.AddWithValue("$performer", performerId);
            command.Parameters.AddWithValue("$album", albumId);
            command.Parameters.AddWithValue("$path", song.Path);
            command.Parameters.AddWithValue("$title", song.Title);
            command.Parameters.AddWithValue("$track", song.Track);
            command.Parameters.AddWithValue("$year", song.Year);
            command.Parameters.AddWithValue("$genre", song.Genre[0]);

            command.ExecuteNonQuery();

            command.CommandText = """
            SELECT last_insert_rowid();
            """; 

            result = command.ExecuteScalar();
        }

        if (result is long songId)
            return songId;

        throw new InvalidOperationException(
            "No se pudo insertar la canción."
            );  

    }

    public long GetOrCreatePerformer(string artistSong, string dataBasePath)
    {
        //Abrimos la conexion a la base de datos
        using var connection = new SqliteConnection($"Data Source = {dataBasePath}"); 
        connection.Open(); 

        using var command = connection.CreateCommand();
        //Consulatamos si existe el performer en la base de datos
        command.CommandText = """
        SELECT id_performer FROM performers
        WHERE name = $name
        LIMIT 1;
        """;

        command.Parameters.AddWithValue("$name", artistSong);

        object? result = command.ExecuteScalar();
    
        if(result == null)
        {
            //El performer no se encuentra en la base de datos.
            //Los creamos y los insertamos.
            command.CommandText = """
            INSERT INTO performers (id_type, name)
            VALUES ($id_type, $nameP); 
            """;
            command.Parameters.AddWithValue("$id_type", 2);
            command.Parameters.AddWithValue("$nameP", artistSong);

            command.ExecuteNonQuery();

            command.CommandText = """
            SELECT last_insert_rowid();
            """; 

            result = command.ExecuteScalar();
        }
        if (result is long performerId)
            return performerId;

        throw new InvalidOperationException(
            "No se pudo obtener el ID del performer."
            ); 
    }

    public long GetOrCreateAlbum(string albumName, string dataBasePath, string albumPath, int year)
    {
        using var connection = new SqliteConnection($"Data Source = {dataBasePath}"); 
        connection.Open(); 

        using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT id_album FROM albums
        WHERE name = $name
        LIMIT 1;
        """;

        command.Parameters.AddWithValue("$name", albumName);
        object? result = command.ExecuteScalar();

        if(result == null)
        {
            //El performer no se encuentra en la base de datos.
            //Los creamos y los insertamos.
            command.CommandText = """
            INSERT INTO albums (path, name, year)
            VALUES ($id_path, $id_name, $id_year); 
            """;
            command.Parameters.AddWithValue("$id_path", albumPath);
            command.Parameters.AddWithValue("$id_name", albumName);
            command.Parameters.AddWithValue("$id_year", year);

            command.ExecuteNonQuery();

            command.CommandText = """
            SELECT last_insert_rowid();
            """; 

            result = command.ExecuteScalar();
        }
        if (result is long albumId)
            return albumId;

        throw new InvalidOperationException(
            "No se pudo obtener el ID del álbum."
            );
    }



}