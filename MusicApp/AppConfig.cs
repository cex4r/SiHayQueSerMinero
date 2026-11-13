namespace MusicApp;

/// <summary>
/// Rutas configurables. Prioridad: argumento de línea de comandos, variable de entorno, valor por defecto.
/// Nada depende de rutas absolutas de desarrollo.
/// </summary>
public sealed class AppConfig
{
    public required string DbPath { get; init; }
    public required string MinerPath { get; init; }

    public static AppConfig Load(string[] args)
    {
        var db = ArgValue(args, "--db") ?? Environment.GetEnvironmentVariable("MUSICAPP_DB");
        var miner = ArgValue(args, "--miner") ?? Environment.GetEnvironmentVariable("MUSICMINER_PATH");

        // En Linux LocalApplicationData es ~/.local/share
        db ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MusicApp", "rolas.db");
        miner ??= FindMiner();

        return new AppConfig { DbPath = Path.GetFullPath(db), MinerPath = Path.GetFullPath(miner) };
    }

    private static string? ArgValue(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
            if (args[i] == name) return args[i + 1];
        return null;
    }

    // Estructura de publicación conocida: MusicApp y MusicMiner publicados como carpetas hermanas,
    // o MusicMiner dentro de la carpeta de MusicApp.
    private static string FindMiner()
    {
        var b = AppContext.BaseDirectory;
        string[] candidates =
        {
            Path.Combine(b, "MusicMiner", "MusicMiner"),
            Path.Combine(b, "MusicMiner", "MusicMiner.dll"),
            Path.Combine(b, "..", "MusicMiner", "MusicMiner"),
            Path.Combine(b, "..", "MusicMiner", "MusicMiner.dll"),
        };
        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }
}
