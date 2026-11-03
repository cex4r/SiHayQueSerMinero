
class Miner
{
    private readonly Search search = new Search();
    private readonly Analyze analyze = new Analyze();
    private readonly AddDataBase database = new AddDataBase();

    public string Mina(
        string sourceDirectory,
        string dataBasePath)
    {
        int processed = 0;
        int failed = 0;

        try
        {
            // Buscamos archivos MP3.
            var paths = search.FindMp3Files(sourceDirectory);

            foreach (string path in paths)
            {
                try
                {
                    // Analizamos el archivo.
                    Song song = analyze.ReadMeData(path);   

                    Console.WriteLine($"Title: {song.Title ?? "NULL"}");
                    Console.WriteLine($"Album: {song.Album ?? "NULL"}");
                    Console.WriteLine($"Year: {song.Year}");
                    Console.WriteLine($"Track: {song.Track}");
                    Console.WriteLine($"Genre: {string.Join(", ", song.Genre)}");



                    // Obtenemos la carpeta del MP3.
                    string albumPath =
                        Path.GetDirectoryName(path)
                        ?? sourceDirectory;

                    // Guardamos la canción.
                    database.GetOrCreateSong(
                        song,
                        dataBasePath,
                        albumPath
                    );

                    processed++;
                }
                catch (Exception ex)
                {
                    failed++;

                    Console.Error.WriteLine(
                    $"Error en {path}: {ex}"
                    );
                }
            }

            return $"""
                Minado terminado.
                Procesadas: {processed}
                Fallidas: {failed}
                """;
        }
        catch (Exception ex)
        {
            return $"Error general del minero: {ex.Message}";
        }
    }
}
