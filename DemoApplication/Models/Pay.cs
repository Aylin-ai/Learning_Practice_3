using DemoApplication.ViewModels;
using ReactiveUI;

namespace DemoApplication.Models;

public class Pay : ViewModelBase
{
    private double _costOfCompanyServiceForCustomerSeller;
    public double CostOfCompanyServiceForCustomerSeller
    {
        get => _costOfCompanyServiceForCustomerSeller;
        set => this.RaiseAndSetIfChanged(ref _costOfCompanyServiceForCustomerSeller, value);
    }
    
    private double _costOfCompanyServiceForCustomerBuyer;
    public double CostOfCompanyServiceForCustomerBuyer
    {
        get => _costOfCompanyServiceForCustomerBuyer;
        set => this.RaiseAndSetIfChanged(ref _costOfCompanyServiceForCustomerBuyer, value);
    }
    
    private double _costOfRealtorServiceForCustomerSeller;
    public double CostOfRealtorServiceForCustomerSeller
    {
        get => _costOfRealtorServiceForCustomerSeller;
        set => this.RaiseAndSetIfChanged(ref _costOfRealtorServiceForCustomerSeller, value);
    }
    
    private double _costOfRealtorServiceForCustomerBuyer;
    public double CostOfRealtorServiceForCustomerBuyer
    {
        get => _costOfRealtorServiceForCustomerBuyer;
        set => this.RaiseAndSetIfChanged(ref _costOfRealtorServiceForCustomerBuyer, value);
    }
}