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
    public void NavigateToMainMenuCommand(object parameter)
    {
        CurrentViewModel = new MainMenuViewModel();
    }
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
    public void NavigateToCreateClientCommand(object parameter)
    {
        CurrentViewModel = new CreateClientViewModel();
    }
    public void NavigateToCreateSupplyCommand(object parameter)
    {
        CurrentViewModel = new CreateSupplyViewModel();
    }
    public void NavigateToCreateDealCommand(object parameter)
    {
        CurrentViewModel = new CreateDealViewModel();
    }
    public void NavigateToCreateDemandCommand(object parameter)
    {
        CurrentViewModel = new CreateDemandViewModel();
    }
    public void NavigateToCreateRealEstateCommand(object parameter)
    {
        CurrentViewModel = new CreateRealEstateViewModel();
    }
    public void NavigateToCreateRealtorCommand(object parameter)
    {
        CurrentViewModel = new CreateRealtorViewModel();
    }
    #endregion

    #endregion

}