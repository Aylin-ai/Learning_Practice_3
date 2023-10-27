using DemoApplication.Infrastructure.Commands.Base;
using DemoApplication.Services;
using DemoApplication.ViewModels;

namespace DemoApplication.Infrastructure.Commands;

public class NavigateCommand<TViewModel> : Command
    where TViewModel : ViewModelBase
{
    private readonly NavigationService<TViewModel> _navigationService;

    public NavigateCommand(NavigationService<TViewModel> navigationService)
    {
        _navigationService = navigationService;
    }

    public override void Execute(object? parameter)
    {
        _navigationService.Navigate();
    }
}