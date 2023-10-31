using DemoApplication.ViewModels;
using ReactiveUI;

namespace DemoApplication.Models;

public class RealEstateMoreInformation : ViewModelBase
{
    private int? _floor;
    public int? Floor
    {
        get => _floor;
        set => this.RaiseAndSetIfChanged(ref _floor, value);
    }

    private int? _rooms;
    public int? Rooms
    {
        get => _rooms;
        set => this.RaiseAndSetIfChanged(ref _rooms, value);
    }

    private float? _totalArea;
    public float? TotalArea
    {
        get => _totalArea;
        set => this.RaiseAndSetIfChanged(ref _totalArea, value);
    }
}