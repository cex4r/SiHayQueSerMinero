using Avalonia.Controls;
using Avalonia.Interactivity;
using MusicApp.Controller;
using MusicApp.Model;

namespace MusicApp.View;

public partial class SongForm : UserControl
{
    private readonly LibraryController _library;
    private readonly Song? _existing;

    public event Action? Done;
    public event Action? Back;

    public SongForm(LibraryController library, Song? existing = null)
    {
        _library = library;
        _existing = existing;
        InitializeComponent();

        if (existing is not null)
        {
            Heading.Text = $"Edit - Song - {existing.Title}";
            TitleBox.Text = existing.Title;
            ArtistBox.Text = existing.Artist;
            // El álbum 0 es el valor "Unknown" del esquema: se deja vacío en el formulario.
            AlbumBox.Text = existing.AlbumId == 0 ? "" : existing.Album;
            PathBox.Text = existing.Path;
            GenreBox.Text = existing.Genre;
            TrackBox.Text = existing.Track?.ToString();
            YearBox.Text = existing.Year?.ToString();
        }
    }

    private void OnDone(object? sender, RoutedEventArgs e)
    {
        var input = new SongInput(
            TitleBox.Text ?? "", ArtistBox.Text ?? "", AlbumBox.Text ?? "", PathBox.Text ?? "",
            GenreBox.Text ?? "", TrackBox.Text ?? "", YearBox.Text ?? "");

        var result = _existing is null
            ? _library.AddSong(input)
            : _library.UpdateSong(_existing.Id, input);

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
