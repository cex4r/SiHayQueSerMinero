using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MusicApp.Controller;
using MusicApp.Model;

namespace MusicApp.View;

/// <summary>Busca un registro, lo selecciona y abre el formulario correspondiente en modo edición (por id real).</summary>
public partial class EditView : UserControl
{
    private readonly Finder _finder;
    private readonly LibraryController _library;

    public event Action? Closed;

    public EditView(Finder finder, LibraryController library)
    {
        _finder = finder;
        _library = library;
        InitializeComponent();
    }

    private void OnBack(object? sender, RoutedEventArgs e) => Closed?.Invoke();

    private void OnQueryKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) RunSearch();
    }

    private void OnSearch(object? sender, RoutedEventArgs e) => RunSearch();

    private void RunSearch()
    {
        var type = Kind.SelectedIndex switch
        {
            1 => SearchType.Song,
            2 => SearchType.Album,
            _ => SearchType.Artist
        };
        var text = Query.Text ?? "";
        var result = _finder.FindGlobal(text, type);

        Results.ItemsSource = null;
        Status.Classes.Remove("error");

        if (!result.Success)
        {
            Status.Classes.Add("error");
            Status.Text = result.Message;
            return;
        }

        var rows = ResultRow.From(result.Value!);
        Status.Text = rows.Count == 0 ? $"No se encontraron resultados para \"{text.Trim()}\"." : "";
        Results.ItemsSource = rows;
    }

    private void OnSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (Results.SelectedItem is not ResultRow row) return;

        switch (row.Source)
        {
            case Artist a:
                var artistForm = new ArtistForm(_library, a);
                artistForm.Done += () => Closed?.Invoke();
                artistForm.Back += ReturnToSearch;
                OpenForm(artistForm);
                break;
            case Song s:
                var songForm = new SongForm(_library, s);
                songForm.Done += () => Closed?.Invoke();
                songForm.Back += ReturnToSearch;
                OpenForm(songForm);
                break;
            case Album al:
                var albumForm = new AlbumForm(_library, al);
                albumForm.Done += () => Closed?.Invoke();
                albumForm.Back += ReturnToSearch;
                OpenForm(albumForm);
                break;
        }
    }

    private void OpenForm(Control form)
    {
        SearchCard.IsVisible = false;
        FormHost.Content = form;
    }

    private void ReturnToSearch()
    {
        FormHost.Content = null;
        Results.SelectedItem = null;
        SearchCard.IsVisible = true;
    }
}
