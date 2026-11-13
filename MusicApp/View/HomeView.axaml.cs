using Avalonia.Controls;
using Avalonia.Interactivity;
using MusicApp.Controller;

namespace MusicApp.View;

public partial class HomeView : UserControl
{
    private readonly Finder _finder;
    private readonly LibraryController _library;

    public HomeView(Finder finder, LibraryController library)
    {
        _finder = finder;
        _library = library;
        InitializeComponent();
    }

    private void OnEdit(object? sender, RoutedEventArgs e)
    {
        var edit = new EditView(_finder, _library);
        edit.Closed += () =>
        {
            OverlayLayer.IsVisible = false;
            Overlay.Content = null;
        };
        Overlay.Content = edit;
        OverlayLayer.IsVisible = true;
    }

    private void OnClose(object? sender, RoutedEventArgs e) =>
        (TopLevel.GetTopLevel(this) as Window)?.Close();
}
