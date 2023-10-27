using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DemoApplication.Infrastructure.Stores;
using DemoApplication.ViewModels;
using DemoApplication.ViewModels.PageViewModels;
using DemoApplication.Views;

namespace DemoApplication;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            NavigationStore navigationStore = new NavigationStore();
            navigationStore.CurrentViewModel = new MainMenuViewModel();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(navigationStore),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}