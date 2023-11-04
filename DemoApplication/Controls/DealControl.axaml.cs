using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DemoApplication.Models;

namespace DemoApplication.Controls;

public partial class DealControl : UserControl
{
    public static readonly StyledProperty<Deal> DealProperty =
        AvaloniaProperty.Register<DealControl, Deal>(nameof(Deal));

    public Deal Deal
    {
        get => GetValue(DealProperty);
        set => SetValue(DealProperty, value);
    }

    public DealControl()
    {
        AvaloniaXamlLoader.Load(this);
    }
}