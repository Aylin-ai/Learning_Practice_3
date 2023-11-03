using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DemoApplication.Models;

namespace DemoApplication.Controls;

public partial class RealEstateControl : UserControl
{
    public static readonly StyledProperty<RealEstate> RealEstateProperty =
        AvaloniaProperty.Register<RealEstateControl, RealEstate>(nameof(RealEstate));

    public RealEstate RealEstate
    {
        get => GetValue(RealEstateProperty);
        set => SetValue(RealEstateProperty, value);
    }

    public RealEstateControl()
    {
        AvaloniaXamlLoader.Load(this);
    }
}