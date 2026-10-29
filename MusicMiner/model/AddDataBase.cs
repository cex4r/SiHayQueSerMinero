using Microsoft.Data.Sqlite;

class AddDataBase
{
    public long InsertSong(Song song, string dataBasePath)
    {  
        // Tu implementación

        //Abrimos una conexion a practica db.
        using var connection = new SqliteConnection($"Data Source={dataBasePath}");
        connection.Open();

        //Preparamos intruccion SQL INSERT INTO Songs.
        using var command = connection.CreateCommand();

        //Usamos los parámetros SQL $title y $year. 
        command.CommandText = """
        INSERT INTO rolas (id_performer, id_album, path, title, track, year, genre)
        VALUES ($performer, $album, $path, $title, $track, $year, $genre);
        """;

        command.Parameters.AddWithValue("$performer", 2);
        command.Parameters.AddWithValue("$album", 1);
        command.Parameters.AddWithValue("$path", song.Path);
        command.Parameters.AddWithValue("$title", song.Title);
        command.Parameters.AddWithValue("$track", song.Track);
        command.Parameters.AddWithValue("$year", song.Year);
        command.Parameters.AddWithValue("$genre", song.Genre[0]);

        command.ExecuteNonQuery();

        command.CommandText = """
        SELECT last_insert_rowid();
        """;
        long lastId = (long)command.ExecuteScalar();
        return lastId; 

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
        
        return (long)result; 
    }



}