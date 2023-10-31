using DemoApplication.ViewModels;
using ReactiveUI;

namespace DemoApplication.Models;

public class Supply : ViewModelBase
{
    private int _id;
    public int Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private int _cost;
    public int Cost
    {
        get => _cost;
        set => this.RaiseAndSetIfChanged(ref _cost, value);
    }

    private Client _client;
    public Client Client
    {
        get => _client;
        set => this.RaiseAndSetIfChanged(ref _client, value);
    }

    private Realtor? _realtor;
    public Realtor? Realtor
    {
        get => _realtor;
        set => this.RaiseAndSetIfChanged(ref _realtor, value);
    }

    private RealEstate _realEstate;
    public RealEstate RealEstate
    {
        get => _realEstate;
        set => this.RaiseAndSetIfChanged(ref _realEstate, value);
    }
}