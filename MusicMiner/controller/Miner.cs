
class Miner
{
    private readonly Search search = new Search();
    private readonly Analyze analyze = new Analyze();
    private readonly AddDataBase database = new AddDataBase();

    public MiningResult Mina(
        string sourceDirectory,
        string dataBasePath)
    {
        MiningResult result = new MiningResult();

        try
        {
            var paths = search.FindMp3Files(sourceDirectory);

            foreach (string path in paths)
            {
                result.Found++;

                try
                {
                    Song song = analyze.ReadMeData(path);

                    // Comprobamos los metadatos obligatorios.
                    if (string.IsNullOrWhiteSpace(song.Title) ||
                        string.IsNullOrWhiteSpace(song.Album) ||
                        song.Artist.Length == 0 ||
                        string.IsNullOrWhiteSpace(song.Artist[0]))
                    {
                        result.Skipped++;

                        result.Errors.Add(
                            $"{Path.GetFileName(path)}: " +
                            "Metadatos obligatorios incompletos."
                        );

                        continue;
                    }

                    string albumPath =
                        Path.GetDirectoryName(path)
                        ?? sourceDirectory;

                    database.GetOrCreateSong(
                        song,
                        dataBasePath,
                        albumPath
                    );

                    result.Registered++;
                }
                catch (Exception ex)
                {
                    result.Skipped++;

                    result.Errors.Add(
                        $"{Path.GetFileName(path)}: {ex.Message}"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add(
                $"Error general del minero: {ex.Message}"
            );
        }

        return result;
    }
}
