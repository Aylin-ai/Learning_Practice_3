using DemoApplication.ViewModels;
using ReactiveUI;

namespace DemoApplication.Models;

public class Realtor : ViewModelBase
{
    private int _id;
    public int Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }
    
    private string? _firstName;
    public string? FirstName
    {
        get => _firstName;
        set => this.RaiseAndSetIfChanged(ref _firstName, value);
    }

    private string? _secondName;
    public string? SecondName
    {
        get => _secondName;
        set => this.RaiseAndSetIfChanged(ref _secondName, value);
    }

    private string? _lastName;
    public string? LastName
    {
        get => _lastName;
        set => this.RaiseAndSetIfChanged(ref _lastName, value);
    }

    private int _share;
    public int Share
    {
        get => _share;
        set => this.RaiseAndSetIfChanged(ref _share, value);
    }
}