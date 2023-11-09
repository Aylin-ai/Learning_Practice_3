using DemoApplication.ViewModels;
using ReactiveUI;

namespace DemoApplication.Models;

public class RealEstate : ViewModelBase
{
    private int _id;
    public int Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private string _type;
    public string Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    private Address _address;
    public Address Address
    {
        get => _address;
        set => this.RaiseAndSetIfChanged(ref _address, value);
    }

    private Coordinates _coordinates;
    public Coordinates Coordinates
    {
        get => _coordinates;
        set => this.RaiseAndSetIfChanged(ref _coordinates, value);
    }

    private RealEstateMoreInformation _moreInformation;
    public RealEstateMoreInformation MoreInformation
    {
        get => _moreInformation;
        set => this.RaiseAndSetIfChanged(ref _moreInformation, value);
    }
}