using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MusicApp.Controller;

namespace MusicApp.View;

public partial class SearchView : UserControl
{
    private readonly Finder _finder;

    public SearchView(Finder finder)
    {
        _finder = finder;
        InitializeComponent();
        Status.Text = "Escribe el nombre de una canción, intérprete o álbum.";
    }

    private void OnSearch(object? sender, RoutedEventArgs e) => RunSearch();

    private void OnQueryKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) RunSearch();
    }

    private void RunSearch()
    {
        var type = Kind.SelectedIndex switch
        {
            1 => SearchType.Artist,
            2 => SearchType.Album,
            _ => SearchType.Song
        };
        var text = Query.Text ?? "";
        var result = _finder.FindGlobal(text, type);

        Results.ItemsSource = null;
        Heading.IsVisible = false;
        Status.Classes.Remove("error");

        if (!result.Success)
        {
            Status.Classes.Add("error");
            Status.Text = result.Message;
            return;
        }

        var rows = ResultRow.From(result.Value!);
        if (rows.Count == 0)
        {
            Status.Text = $"No se encontraron resultados para \"{text.Trim()}\".";
            return;
        }

        Status.Text = "";
        Heading.IsVisible = true;
        Results.ItemsSource = rows;
    }
}
