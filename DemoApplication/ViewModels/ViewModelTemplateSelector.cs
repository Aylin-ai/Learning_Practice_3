using System;
using System.Globalization;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using DemoApplication.ViewModels.PageViewModels;
using DemoApplication.Views.Pages;

namespace DemoApplication.ViewModels;

public class ViewModelTemplateSelector : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ClientsViewModel)
        {
            return new Clients();
        }
        else if (value is CreateClientViewModel)
        {
            return new CreateClient();
        }
        else if (value is CreateDealViewModel)
            return new CreateDeal();
        else if (value is CreateDemandViewModel)
            return new CreateDemand();
        else if (value is CreateRealEstate)
            return new CreateRealEstate();
        else if (value is CreateRealtor)
            return new CreateRealtor();
        else if (value is CreateSupplyViewModel)
            return new CreateSupply();
        else if (value is DealsViewModel)
            return new Deals();
        else if (value is DemandsViewModel)
            return new Demands();
        else if (value is MainMenuViewModel)
            return new MainMenu();
        else if (value is RealEstatesViewModel)
            return new RealEstates();
        else if (value is RealtorsViewModel)
            return new Realtors();
        else if (value is SuppliesViewModel)
            return new Supplies();
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}