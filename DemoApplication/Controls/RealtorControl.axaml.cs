using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DemoApplication.Models;

namespace DemoApplication.Controls;

public partial class RealtorControl : UserControl
{
    public static readonly StyledProperty<Realtor> RealtorProperty =
        AvaloniaProperty.Register<RealtorControl, Realtor>(nameof(Realtor));

    public Realtor Realtor
    {
        get => GetValue(RealtorProperty);
        set => SetValue(RealtorProperty, value);
    }

    public RealtorControl()
    {
        AvaloniaXamlLoader.Load(this);
    }
}