using Avalonia.Controls;
using Avalonia.Interactivity;
using MusicApp.Controller;

namespace MusicApp.View;

public partial class MainWindow : Window
{
    private readonly Finder _finder;
    private readonly LibraryController _library;
    private readonly MiningController _mining;

    public MainWindow(AppConfig config)
    {
        _finder = new Finder(config);
        _library = new LibraryController(config);
        _mining = new MiningController(config);
        InitializeComponent();
        Navigate(new HomeView(_finder, _library), HomeTab);
    }

    private void OnSearchTab(object? sender, RoutedEventArgs e) => Navigate(new SearchView(_finder), SearchTab);
    private void OnHomeTab(object? sender, RoutedEventArgs e) => Navigate(new HomeView(_finder, _library), HomeTab);
    private void OnLibraryTab(object? sender, RoutedEventArgs e) => Navigate(new LibraryView(_finder, _library, _mining), LibraryTab);

    // Cada navegación crea la vista de nuevo, así siempre muestra los datos actuales de SQLite.
    private void Navigate(Control view, Button activeTab)
    {
        Host.Content = view;
        foreach (var tab in new[] { SearchTab, HomeTab, LibraryTab })
        {
            if (tab == activeTab)
            {
                if (!tab.Classes.Contains("active")) tab.Classes.Add("active");
            }
            else
            {
                tab.Classes.Remove("active");
            }
        }
    }
}
