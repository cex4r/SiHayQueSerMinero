using Avalonia.Controls;
using Avalonia.Interactivity;
using MusicApp.Controller;
using MusicApp.Model;

namespace MusicApp.View;

/// <summary>Agregar (existing == null) o editar un intérprete. Para Person hay un segundo paso con datos de persons.</summary>
public partial class ArtistForm : UserControl
{
    private readonly LibraryController _library;
    private readonly Artist? _existing;
    private ArtistType _type = ArtistType.Unknown;
    private bool _personStep;

    public event Action? Done;
    public event Action? Back;

    public ArtistForm(LibraryController library, Artist? existing = null)
    {
        _library = library;
        _existing = existing;
        InitializeComponent();

        if (existing is not null)
        {
            Heading.Text = $"Edit - Artist - {existing.Name}";
            NameBox.Text = existing.Name;
            StageBox.Text = existing.Person?.StageName;
            RealBox.Text = existing.Person?.RealName;
            BirthBox.Text = existing.Person?.BirthDate;
            DeathBox.Text = existing.Person?.DeathDate;
        }
        SetType(existing?.Type ?? ArtistType.Unknown);
    }

    private void SetType(ArtistType type)
    {
        _type = type;
        PersonButton.IsChecked = type == ArtistType.Person;
        GroupButton.IsChecked = type == ArtistType.Group;
        UnknownButton.IsChecked = type == ArtistType.Unknown;
        // Con Person el botón avanza al paso de datos personales en lugar de guardar.
        DoneButton.Content = type == ArtistType.Person ? "Next" : "Done";
    }

    private void ShowPersonStep(bool show)
    {
        _personStep = show;
        BasicPanel.IsVisible = !show;
        PersonPanel.IsVisible = show;
        DoneButton.Content = show ? "Done" : (_type == ArtistType.Person ? "Next" : "Done");
        var prefix = _existing is null ? "Add - Artist" : $"Edit - Artist - {_existing.Name}";
        Heading.Text = show ? $"{prefix} - Person" : prefix;
        Error.IsVisible = false;
    }

    private void OnType(object? sender, RoutedEventArgs e)
    {
        if (sender == PersonButton) SetType(ArtistType.Person);
        else if (sender == GroupButton) SetType(ArtistType.Group);
        else SetType(ArtistType.Unknown);
    }

    private void OnDone(object? sender, RoutedEventArgs e)
    {
        if (_type == ArtistType.Person && !_personStep)
        {
            ShowPersonStep(true);
            return;
        }
        Submit();
    }

    private void OnBack(object? sender, RoutedEventArgs e)
    {
        if (_personStep)
        {
            ShowPersonStep(false);
            return;
        }
        Back?.Invoke();
    }

    private void Submit()
    {
        var input = new ArtistInput(
            NameBox.Text ?? "", _type,
            StageBox.Text ?? "", RealBox.Text ?? "", BirthBox.Text ?? "", DeathBox.Text ?? "");

        var result = _existing is null
            ? _library.AddArtist(input)
            : _library.UpdateArtist(_existing.Id, input);

        if (!result.Success)
        {
            Error.Text = result.Message;
            Error.IsVisible = true;
            return;
        }
        Done?.Invoke();
    }
}
