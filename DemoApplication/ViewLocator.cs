using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using DemoApplication.Models;
using DemoApplication.ViewModels;

namespace DemoApplication;

public class ViewLocator : IDataTemplate
{
    public Control Build(object data)
    {
        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        var dataType = data.GetType();

        if (dataType == typeof(Client))
        {
            return new ContentControl
            {
                ContentTemplate = (IDataTemplate)Application.Current.Resources["ClientTemplate"],
                Content = data
            };
        }
        else if (dataType == typeof(Supply))
        {
            return new ContentControl
            {
                ContentTemplate = (IDataTemplate)Application.Current.Resources["SupplyTemplate"],
                Content = data
            };
        }
        else if (dataType == typeof(Demand))
        {
            return new ContentControl
            {
                ContentTemplate = (IDataTemplate)Application.Current.Resources["DemandTemplate"],
                Content = data
            };
        }
        else if (dataType == typeof(RealEstate))
        {
            return new ContentControl
            {
                ContentTemplate = (IDataTemplate)Application.Current.Resources["RealEstateTemplate"],
                Content = data
            };
        }
        else if (dataType == typeof(Deal))
        {
            return new ContentControl
            {
                ContentTemplate = (IDataTemplate)Application.Current.Resources["DealTemplate"],
                Content = data
            };
        }
        else if (dataType == typeof(Realtor))
        {
            return new ContentControl
            {
                ContentTemplate = (IDataTemplate)Application.Current.Resources["RealtorTemplate"],
                Content = data
            };
        }
        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object data)
    {
        return data is ViewModelBase;
    }
}