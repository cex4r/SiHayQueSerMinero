using Avalonia.Controls;
using Avalonia.Interactivity;
using MusicApp.Controller;
using MusicApp.Model;

namespace MusicApp.View;

public partial class AlbumForm : UserControl
{
    private readonly LibraryController _library;
    private readonly Album? _existing;

    public event Action? Done;
    public event Action? Back;

    public AlbumForm(LibraryController library, Album? existing = null)
    {
        _library = library;
        _existing = existing;
        InitializeComponent();

        if (existing is not null)
        {
            Heading.Text = $"Edit - Album - {existing.Name}";
            NameBox.Text = existing.Name;
            PathBox.Text = existing.Path;
            YearBox.Text = existing.Year?.ToString();
        }
    }

    private void OnDone(object? sender, RoutedEventArgs e)
    {
        var input = new AlbumInput(NameBox.Text ?? "", PathBox.Text ?? "", YearBox.Text ?? "");

        var result = _existing is null
            ? _library.AddAlbum(input)
            : _library.UpdateAlbum(_existing.Id, input);

        if (!result.Success)
        {
            Error.Text = result.Message;
            Error.IsVisible = true;
            return;
        }
        Done?.Invoke();
    }

    private void OnBack(object? sender, RoutedEventArgs e) => Back?.Invoke();
}
