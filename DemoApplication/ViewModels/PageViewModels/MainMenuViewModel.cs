using System.Windows.Input;
using DemoApplication.Infrastructure.Commands;
using DemoApplication.Infrastructure.Stores;

namespace DemoApplication.ViewModels.PageViewModels;

public class MainMenuViewModel : ViewModelBase
{
    public string Greetings { get; set; } = "greetings";
    public MainMenuViewModel()
    {
    }
    
}