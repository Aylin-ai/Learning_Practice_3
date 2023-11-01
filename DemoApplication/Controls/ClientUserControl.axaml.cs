using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DemoApplication.Models;

namespace DemoApplication.Controls;

public partial class ClientUserControl : UserControl
{
    public static readonly StyledProperty<Client> ClientProperty =
        AvaloniaProperty.Register<ClientUserControl, Client>(nameof(Client));

    public Client Client
    {
        get => GetValue(ClientProperty);
        set => SetValue(ClientProperty, value);
    }

    public ClientUserControl()
    {
        AvaloniaXamlLoader.Load(this);
    }
}