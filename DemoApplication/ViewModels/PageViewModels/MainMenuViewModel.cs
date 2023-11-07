using System;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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