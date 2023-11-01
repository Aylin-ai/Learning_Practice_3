using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DemoApplication.Models;

namespace DemoApplication.Controls;

public partial class DemandControl : UserControl
{
    public static readonly StyledProperty<Demand> DemandProperty =
        AvaloniaProperty.Register<DemandControl, Demand>(nameof(Demand));

    public Demand Demand
    {
        get => GetValue(DemandProperty);
        set => SetValue(DemandProperty, value);
    }

    public DemandControl()
    {
        AvaloniaXamlLoader.Load(this);
    }
}