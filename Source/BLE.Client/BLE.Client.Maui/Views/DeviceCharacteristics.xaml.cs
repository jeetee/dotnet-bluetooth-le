using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.Maui.Views;

public partial class DeviceCharacteristics : ContentPage
{
	public DeviceCharacteristics()
	{
		InitializeComponent();
	}

    private async void SubscribeToCharacteristicUpdates_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
		if (sender is not null)
		{
			ICharacteristic characteristic = (ICharacteristic)((CheckBox)sender).BindingContext;
			if (e.Value == true)
			{
				try
				{
					await characteristic.StartUpdatesAsync();
                    App.Logger.AddMessage($"Subscribed to {characteristic.Id}");
                }
				catch (Exception ex)
				{
					App.Logger.AddMessage($"Failed to Subscribe to {characteristic.Id}\n{ex}");
					App.AlertSvc.ShowAlert("Failed to Subscribe", ex.ToString());
				}
			}
			else
			{
                try
                {
                    await characteristic.StopUpdatesAsync();
                    App.Logger.AddMessage($"Unsubscribed from {characteristic.Id}");
                }
                catch (Exception ex)
                {
                    App.Logger.AddMessage($"Failed to Unsubscribe from {characteristic.Id}\n{ex}");
                    App.AlertSvc.ShowAlert("Failed to Unsubscribe", ex.ToString());
                }
            }
		}
    }
}
