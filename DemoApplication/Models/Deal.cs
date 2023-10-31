using DemoApplication.ViewModels;
using ReactiveUI;

namespace DemoApplication.Models;

public class Deal : ViewModelBase
{
    private int _id;
    public int Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private Demand _demand;
    public Demand Demand
    {
        get => _demand;
        set => this.RaiseAndSetIfChanged(ref _demand, value);
    }

    private Supply _supply;
    public Supply Supply
    {
        get => _supply;
        set => this.RaiseAndSetIfChanged(ref _supply, value);
    }
}