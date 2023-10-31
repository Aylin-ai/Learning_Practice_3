using System.Reactive;
using System.Windows.Input;
using DemoApplication.Infrastructure.Stores;
using DemoApplication.ViewModels.PageViewModels;
using ReactiveUI;

namespace DemoApplication.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel;
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }
    
    public MainWindowViewModel()
    {
        _currentViewModel = new MainMenuViewModel();
    }

    #region Команды

    #region Команды навигации
    
    public void NavigateToRealEstatesCommand(object parameter)
    {
        CurrentViewModel = new RealEstatesViewModel();
    }
    public void NavigateToDemandsCommand(object parameter)
    {
        CurrentViewModel = new DemandsViewModel();
    }
    public void NavigateToSuppliesCommand(object parameter)
    {
        CurrentViewModel = new SuppliesViewModel();
    }
    public void NavigateToDealsCommand(object parameter)
    {
        CurrentViewModel = new DealsViewModel();
    }
    public void NavigateToClientsCommand(object parameter)
    {
        CurrentViewModel = new ClientsViewModel();
    }
    public void NavigateToRealtorsCommand(object parameter)
    {
        CurrentViewModel = new RealtorsViewModel();
    }
    #endregion

    #endregion

}