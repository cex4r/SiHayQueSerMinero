using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace MusicApp.Controller;

/// <summary>Resultado estructurado que MusicMiner imprime como JSON (ver README, contrato de línea de comandos).</summary>
public sealed class MiningReport
{
    public int Found { get; init; }
    public int Inserted { get; init; }
    public int Duplicates { get; init; }
    public int Skipped { get; init; }
    public int Failed { get; init; }
    public List<string> Reasons { get; init; } = [];
}

public sealed class MiningController(AppConfig config)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Ejecuta MusicMiner como proceso externo. La selección del directorio es de la vista (selector de carpetas).
    /// Un código de salida 0 sin JSON válido se trata como fallo: que el proceso termine no prueba que minó bien.
    /// </summary>
    public async Task<Result<MiningReport>> MineAsync(string musicDirectory, CancellationToken ct = default)
    {
        if (!Directory.Exists(musicDirectory))
            return Result<MiningReport>.Fail(ErrorKind.Validation, $"El directorio no existe: {musicDirectory}");
        if (!File.Exists(config.DbPath))
            return Result<MiningReport>.Fail(ErrorKind.Connection, $"No existe la base de datos: {config.DbPath}");
        if (!File.Exists(config.MinerPath))
            return Result<MiningReport>.Fail(ErrorKind.Miner,
                $"No se encontró MusicMiner en {config.MinerPath}. Indica su ubicación con --miner o la variable MUSICMINER_PATH.");

        try
        {
            using var process = new Process { StartInfo = BuildStartInfo(musicDirectory) };
            process.Start();

            // Se leen ambos flujos en paralelo para que el proceso no se bloquee con búferes llenos.
            var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
            var stderrTask = process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);
            var stdout = await stdoutTask;
            var stderr = (await stderrTask).Trim();

            if (process.ExitCode != 0)
            {
                var detail = stderr.Length > 0 ? stderr : "sin detalles";
                return Result<MiningReport>.Fail(ErrorKind.Miner, $"MusicMiner terminó con código {process.ExitCode}: {detail}");
            }

            var report = ParseReport(stdout);
            return report is null
                ? Result<MiningReport>.Fail(ErrorKind.Miner,
                    "MusicMiner terminó sin errores pero no devolvió un resultado estructurado (JSON en la última línea de la salida).")
                : Result<MiningReport>.Ok(report);
        }
        catch (Win32Exception e)
        {
            return Result<MiningReport>.Fail(ErrorKind.Miner, $"No se pudo iniciar MusicMiner: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            return Result<MiningReport>.Fail(ErrorKind.Miner, "El minado fue cancelado.");
        }
    }

    private ProcessStartInfo BuildStartInfo(string musicDirectory)
    {
        var isDll = config.MinerPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
        var psi = new ProcessStartInfo
        {
            FileName = isDll ? "dotnet" : config.MinerPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        // ArgumentList evita problemas de comillas con rutas que tienen espacios.
        if (isDll) psi.ArgumentList.Add(config.MinerPath);
        psi.ArgumentList.Add("--db");
        psi.ArgumentList.Add(config.DbPath);
        psi.ArgumentList.Add("--json");
        psi.ArgumentList.Add(musicDirectory);
        return psi;
    }

    private static MiningReport? ParseReport(string stdout)
    {
        var line = stdout.Split('\n').Select(l => l.Trim()).LastOrDefault(l => l.StartsWith('{') && l.EndsWith('}'));
        if (line is null) return null;
        try
        {
            return JsonSerializer.Deserialize<MiningReport>(line, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
