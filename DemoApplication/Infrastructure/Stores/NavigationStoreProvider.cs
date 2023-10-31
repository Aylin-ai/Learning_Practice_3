namespace DemoApplication.Infrastructure.Stores;

public static class NavigationStoreProvider
{
    private static NavigationStore _navigationStore;

    public static void InitializeNavigationStore(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
    }

    public static NavigationStore GetNavigationStore()
    {
        return _navigationStore;
    }
}
