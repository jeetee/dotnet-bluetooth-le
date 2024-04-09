using BLE.Client.Maui.Services;
using BLE.Client.Maui.ViewModels;

namespace BLE.Client.Maui;

public partial class App : Application
{
    private static IServiceProvider ServicesProvider;
    public static IServiceProvider Services => ServicesProvider;
    private static IAlertService AlertService;
    public static IAlertService AlertSvc => AlertService;

    public readonly static LogService Logger = new();

    private BLEDeviceDetailedViewModel _DeviceVM = null;
    public BLEDeviceDetailedViewModel DeviceVM
    {
        get => _DeviceVM;
        set
        {
            if (value != _DeviceVM)
            {
                _DeviceVM = value;
                if (MainPage != null)
                {
                    MainPage.BindingContext = _DeviceVM;
                }
            }
        }
    }

    public App(IServiceProvider provider)
	{
		InitializeComponent();

        ServicesProvider = provider;
        AlertService = Services.GetService<IAlertService>();
        MainPage = new AppShell();
	}
}
