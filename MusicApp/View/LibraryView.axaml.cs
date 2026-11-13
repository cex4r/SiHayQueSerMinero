using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MusicApp.Controller;

namespace MusicApp.View;

public partial class LibraryView : UserControl
{
    private readonly Finder _finder;
    private readonly LibraryController _library;
    private readonly MiningController _mining;

    public LibraryView(Finder finder, LibraryController library, MiningController mining)
    {
        _finder = finder;
        _library = library;
        _mining = mining;
        InitializeComponent();
        Reload();
    }

    private void Reload()
    {
        var result = _finder.ListSongs();
        Status.Classes.Remove("error");

        if (!result.Success)
        {
            Songs.ItemsSource = null;
            Status.Classes.Add("error");
            Status.Text = result.Message;
            return;
        }

        var rows = result.Value!.Select(s => ResultRow.From(s)).ToList();
        Songs.ItemsSource = rows;
        Status.Text = rows.Count == 0 ? "Tu biblioteca está vacía. Usa Add o Mine para agregar canciones." : "";
    }

    private void ShowOverlay(Control content)
    {
        Overlay.Content = content;
        OverlayLayer.IsVisible = true;
    }

    private void CloseOverlay(bool reload)
    {
        OverlayLayer.IsVisible = false;
        Overlay.Content = null;
        if (reload) Reload();
    }

    // ---- Add ----

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        var menu = new AddMenuView();
        menu.Back += () => CloseOverlay(false);
        menu.ArtistChosen += () => ShowForm(new ArtistForm(_library));
        menu.SongChosen += () => ShowForm(new SongForm(_library));
        menu.AlbumChosen += () => ShowForm(new AlbumForm(_library));
        ShowOverlay(menu);
    }

    // Back vuelve al menú Add; Done cierra y recarga la lista.
    private void ShowForm(Control form)
    {
        switch (form)
        {
            case ArtistForm f: f.Done += () => CloseOverlay(true); f.Back += () => OnAdd(null, new RoutedEventArgs()); break;
            case SongForm f: f.Done += () => CloseOverlay(true); f.Back += () => OnAdd(null, new RoutedEventArgs()); break;
            case AlbumForm f: f.Done += () => CloseOverlay(true); f.Back += () => OnAdd(null, new RoutedEventArgs()); break;
        }
        ShowOverlay(form);
    }

    // ---- Mine ----

    private async void OnMine(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Selecciona el directorio con tu música",
            AllowMultiple = false
        });
        if (folders.Count == 0) return;

        var path = folders[0].TryGetLocalPath();
        if (path is null) return;

        MineButton.IsEnabled = false;
        AddButton.IsEnabled = false;
        MineStatus.Text = "Minando...";

        var result = await _mining.MineAsync(path);

        MineStatus.Text = "";
        MineButton.IsEnabled = true;
        AddButton.IsEnabled = true;

        if (result.Success) Reload();
        ShowOverlay(new MiningResultView(result, () => CloseOverlay(false)));
    }
}
