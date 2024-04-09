using BLE.Client.Maui.ViewModels;

namespace BLE.Client.Maui.Views;

public partial class DeviceInfo : ContentPage, IQueryAttributable
{
	public DeviceInfo()
	{
		InitializeComponent();
	}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
		((App)Application.Current).DeviceVM = query[nameof(BLEDeviceDetailedViewModel)] as BLEDeviceDetailedViewModel;
    }
}
