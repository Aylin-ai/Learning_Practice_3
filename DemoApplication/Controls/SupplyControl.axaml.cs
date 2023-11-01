using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DemoApplication.Models;

namespace DemoApplication.Controls;

public partial class SupplyControl : UserControl
{
    public static readonly StyledProperty<Supply> SupplyProperty =
        AvaloniaProperty.Register<SupplyControl, Supply>(nameof(Supply));

    public Supply Supply
    {
        get => GetValue(SupplyProperty);
        set => SetValue(SupplyProperty, value);
    }

    public SupplyControl()
    {
        AvaloniaXamlLoader.Load(this);
    }
}