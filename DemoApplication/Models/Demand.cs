using DemoApplication.ViewModels;
using ReactiveUI;

namespace DemoApplication.Models;

public class Demand : ViewModelBase
{
    private int _id;
    public int Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private string _realEstateType;
    public string RealEstateType
    {
        get => _realEstateType;
        set => this.RaiseAndSetIfChanged(ref _realEstateType, value);
    }

    private int _minCost;
    public int MinCost
    {
        get => _minCost;
        set => this.RaiseAndSetIfChanged(ref _minCost, value);
    }

    private int _maxCost;
    public int MaxCost
    {
        get => _maxCost;
        set => this.RaiseAndSetIfChanged(ref _maxCost, value);
    }

    private Address _address;
    public Address Address
    {
        get => _address;
        set => this.RaiseAndSetIfChanged(ref _address, value);
    }

    private Client _client;
    public Client Client
    {
        get => _client;
        set => this.RaiseAndSetIfChanged(ref _client, value);
    }

    private Realtor _realtor;
    public Realtor Realtor
    {
        get => _realtor;
        set => this.RaiseAndSetIfChanged(ref _realtor, value);
    }

    private DemandMoreInformation? _moreInformation;
    public DemandMoreInformation? MoreInformation
    {
        get => _moreInformation;
        set => this.RaiseAndSetIfChanged(ref _moreInformation, value);
    }
}