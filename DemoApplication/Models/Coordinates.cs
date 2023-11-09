using DemoApplication.ViewModels;
using ReactiveUI;

namespace DemoApplication.Models;

public class Coordinates : ViewModelBase
{
    private float _latitude;
    public float Latitude
    {
        get => _latitude;
        set => this.RaiseAndSetIfChanged(ref _latitude, value);
    }

    private float _longitude;
    public float Longitude
    {
        get => _longitude;
        set => this.RaiseAndSetIfChanged(ref _longitude, value);
    }
}