
using System;
using System.IO;
using System.Text.Json;

class Program
{
    static int Main(string[] args)
    {
        string? databasePath = null;
        string? sourceDirectory = null;
        bool json = false;

        // Interpretar argumentos de la terminal.
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--db":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine(
                            "Falta la ruta después de --db.");
                        return 1;
                    }

                    databasePath = args[++i];
                    break;

                case "--json":
                    json = true;
                    break;

                default:
                    if (args[i].StartsWith("--") ||
                        sourceDirectory is not null)
                    {
                        Console.Error.WriteLine(
                            $"Argumento inválido: {args[i]}");
                        return 1;
                    }

                    sourceDirectory = args[i];
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(databasePath) ||
            string.IsNullOrWhiteSpace(sourceDirectory))
        {
            Console.Error.WriteLine(
                "Uso: MusicMiner --db <base.db> " +
                "[--json] <directorio>");
            return 1;
        }

        databasePath = Path.GetFullPath(databasePath);
        sourceDirectory = Path.GetFullPath(sourceDirectory);

        if (!File.Exists(databasePath))
        {
            Console.Error.WriteLine(
                $"No existe la base de datos: {databasePath}");
            return 1;
        }

        if (!Directory.Exists(sourceDirectory))
        {
            Console.Error.WriteLine(
                $"No existe el directorio: {sourceDirectory}");
            return 1;
        }

        try
        {
            Miner minero = new Miner();

            MiningResult resultado = minero.Mina(
                sourceDirectory,
                databasePath
            );

            if (json)
            {
                var output = new
                {
                    found = resultado.Found,
                    inserted = resultado.Registered,
                    duplicates = 0,
                    skipped = resultado.Skipped,
                    failed = 0,
                    reasons = resultado.Errors
                };

                Console.WriteLine(
                    JsonSerializer.Serialize(output)
                );
            }
            else
            {
                Message vista = new Message();
                vista.ShowResult(resultado);
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Error del minero: {ex.Message}");
            return 1;
        }
    }
}
