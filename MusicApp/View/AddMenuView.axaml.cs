using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MusicApp.View;

public partial class AddMenuView : UserControl
{
    public event Action? ArtistChosen;
    public event Action? SongChosen;
    public event Action? AlbumChosen;
    public event Action? Back;

    public AddMenuView() => InitializeComponent();

    private void OnArtist(object? sender, RoutedEventArgs e) => ArtistChosen?.Invoke();
    private void OnSong(object? sender, RoutedEventArgs e) => SongChosen?.Invoke();
    private void OnAlbum(object? sender, RoutedEventArgs e) => AlbumChosen?.Invoke();
    private void OnBack(object? sender, RoutedEventArgs e) => Back?.Invoke();
}
