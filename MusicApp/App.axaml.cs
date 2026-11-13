using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MusicApp.View;

namespace MusicApp;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var config = AppConfig.Load(desktop.Args ?? []);
            desktop.MainWindow = new MainWindow(config);
        }
        base.OnFrameworkInitializationCompleted();
    }
}
