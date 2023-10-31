using DemoApplication.ViewModels;
using ReactiveUI;

namespace DemoApplication.Models;

public class Client : ViewModelBase
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

    private string? _phone;
    public string? Phone
    {
        get => _phone;
        set => this.RaiseAndSetIfChanged(ref _phone, value);
    }

    private string? _email;

    public string? Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }
}