using DemoApplication.ViewModels;
using ReactiveUI;

namespace DemoApplication.Models;

public class DemandMoreInformation : ViewModelBase
{
    private float _minArea;
    public float MinArea
    {
        get => _minArea;
        set => this.RaiseAndSetIfChanged(ref _minArea, value);
    }
    
    private float _maxArea;
    public float MaxArea
    {
        get => _maxArea;
        set => this.RaiseAndSetIfChanged(ref _maxArea, value);
    }
    
    private int _minRooms;
    public int MinRooms
    {
        get => _minRooms;
        set => this.RaiseAndSetIfChanged(ref _minRooms, value);
    }
    
    private int _maxRooms;
    public int MaxRooms
    {
        get => _maxRooms;
        set => this.RaiseAndSetIfChanged(ref _maxRooms, value);
    }
    
    private int _minFloor;
    public int MinFloor
    {
        get => _minFloor;
        set => this.RaiseAndSetIfChanged(ref _minFloor, value);
    }
    
    private int _maxFloor;
    public int MaxFloor
    {
        get => _maxFloor;
        set => this.RaiseAndSetIfChanged(ref _maxFloor, value);
    }
}