using System;
using DemoApplication.ViewModels;

namespace DemoApplication.Infrastructure.Stores;

public class NavigationStore
{
    public event Action CurrentViewModelChanged;

    private ViewModelBase _currentViewModel;
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            _currentViewModel = value;
            OnCurrenViewModelChanged();
        }
    }

    private void OnCurrenViewModelChanged()
    {
        CurrentViewModelChanged?.Invoke();
    }
}