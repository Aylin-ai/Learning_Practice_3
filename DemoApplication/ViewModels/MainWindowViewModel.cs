using DemoApplication.Infrastructure.Stores;

namespace DemoApplication.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;

    public ViewModelBase MainMenuViewModel { get; set; }
    public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;
    
    public MainWindowViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
    }

    private void OnCurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }

}