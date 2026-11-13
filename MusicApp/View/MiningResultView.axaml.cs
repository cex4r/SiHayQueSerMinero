using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MusicApp.Controller;

namespace MusicApp.View;

/// <summary>Muestra el resultado real de la ejecución de MusicMiner (o el error si falló).</summary>
public partial class MiningResultView : UserControl
{
    private readonly Action _onClose;

    public MiningResultView(Result<MiningReport> result, Action onClose)
    {
        _onClose = onClose;
        InitializeComponent();

        if (!result.Success)
        {
            Heading.Text = "El minado no se completó.";
            Body.Classes.Add("error");
            Body.Text = result.Message;
            return;
        }

        var r = result.Value!;
        Heading.Text = "Minado terminado.";

        var text = new StringBuilder();
        text.AppendLine($"Archivos encontrados: {r.Found}");
        text.AppendLine($"Canciones registradas: {r.Inserted}");
        if (r.Duplicates > 0) text.AppendLine($"Canciones duplicadas: {r.Duplicates}");
        text.AppendLine($"Canciones omitidas: {r.Skipped}");
        if (r.Failed > 0) text.AppendLine($"Canciones con error: {r.Failed}");
        if (r.Reasons.Count > 0)
        {
            text.AppendLine("Motivos:");
            foreach (var reason in r.Reasons) text.AppendLine(reason);
        }
        Body.Text = text.ToString().TrimEnd();
    }

    private void OnClose(object? sender, RoutedEventArgs e) => _onClose();
}
