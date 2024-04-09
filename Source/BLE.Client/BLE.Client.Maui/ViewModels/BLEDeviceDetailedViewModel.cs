using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Windows.Input;

namespace BLE.Client.Maui.ViewModels;

public class BLEDeviceDetailedViewModel : BaseViewModel
{
    private readonly IDevice _device;
    private readonly IAdapter Adapter;

    public string Name => _device.Name;
    public bool Connected => _device.State == DeviceState.Connected;
    public String MacAddress { get; private init; }
    public IReadOnlyList<AdvertisementRecord> Adverts => _device.AdvertisementRecords;

    public BLEDeviceDetailedViewModel(IDevice device)
    {
        _device = device;
        string mac = _device.Id.ToString().Split('-').Last();
        MacAddress = String.Join(":", Enumerable.Range(0, 6).Select(i => mac.Substring(i * 2, 2)));

        Adapter = CrossBluetoothLE.Current?.Adapter;
        Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
        Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;

        DisconnectCommand = new Command(Disconnect);
    }

    #region Disconnect
    private void Adapter_DeviceConnectionLost(object sender, DeviceErrorEventArgs e)
    {
        App.Logger.AddMessage($"DeviceConnectionLost:\n{e.Device.Name}\n{e.Device.Id}\n{e.ErrorMessage}");
        RaisePropertyChanged(nameof(Connected));
    }

    private void Adapter_DeviceDisconnected(object sender, DeviceEventArgs e)
    {
        App.Logger.AddMessage($"DeviceDisconnected:\n{e.Device.Name}\n{e.Device.Id}");
        RaisePropertyChanged(nameof(Connected));
    }

    public ICommand DisconnectCommand { get; init; }
    private async void Disconnect()
    {
        App.Logger.AddMessage($"Disconnecting device:\n{_device.Name}\n{_device.Id}");
        try
        {
            await Adapter.DisconnectDeviceAsync(_device);
            RaisePropertyChanged(nameof(Connected));
        }
        catch (Exception e)
        {
            App.Logger.AddMessage($"Error whilst disconnecting from device:\n{e}\n{_device}");
        }
        await Shell.Current.GoToAsync("//BLEScanner");
    }
    #endregion Disconnect
}
