using DemoApplication.ViewModels;
using ReactiveUI;

namespace DemoApplication.Models;

public class Address : ViewModelBase
{
    private string? _city;
    public string? City
    {
        get => _city;
        set => this.RaiseAndSetIfChanged(ref _city, value);
    }

    private string? _street;
    public string? Street
    {
        get => _street;
        set => this.RaiseAndSetIfChanged(ref _street, value);
    }
    
    private string? _house;
    public string? House
    {
        get => _house;
        set => this.RaiseAndSetIfChanged(ref _house, value);
    }
    
    private int? _apartment;
    public int? Apartment
    {
        get => _apartment;
        set => this.RaiseAndSetIfChanged(ref _apartment, value);
    }
}