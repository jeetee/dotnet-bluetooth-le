﻿using CommunityToolkit.Mvvm.Input;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Windows.Input;

namespace BLE.Client.Maui.ViewModels;

public class CharacteristicsPerService
{
    public IService Service { get; init; }
    public IReadOnlyList<BLECharacteristicViewModel> Characteristics { get; init; }
}

public class BLEDeviceDetailedViewModel : BaseViewModel
{
    private readonly IDevice _device;
    private readonly IAdapter Adapter;

    public string Name => _device.Name;
    public bool Connected => _device.State == DeviceState.Connected;
    public String MacAddress { get; private init; }
    public IReadOnlyList<AdvertisementRecord> Adverts => _device.AdvertisementRecords;

    public IReadOnlyList<IService> _Services = null;
    public IReadOnlyList<IService> Services
    {
        get => _Services;
        private set
        {
            _Services = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(ServiceCount));
        }
    }
    public int ServiceCount => _Services?.Count ?? 0;

    private List<CharacteristicsPerService> _Characteristics;
    public IReadOnlyList<CharacteristicsPerService> Characteristics { get; private set; } = null;

    public BLEDeviceDetailedViewModel(IDevice device)
    {
        _device = device;
        string mac = _device.Id.ToString().Split('-').Last();
        MacAddress = String.Join(":", Enumerable.Range(0, 6).Select(i => mac.Substring(i * 2, 2)));

        Adapter = CrossBluetoothLE.Current?.Adapter;
        Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
        Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;

        DiscoverServicesCommand = new RelayCommand(DiscoverServices, CanDiscoverServices);
        DisconnectCommand = new Command(Disconnect);

        ReadCharacteristicValueCommand = new Command(ReadCharacteristicValue);
    }

    #region Services
    public RelayCommand DiscoverServicesCommand { get; init; }
    CancellationTokenSource _discoveryCancellationTokenSource = null;
    private bool CanDiscoverServices()
    {
        return Connected && (_discoveryCancellationTokenSource == null);
    }
    private async void DiscoverServices()
    {
        if (_discoveryCancellationTokenSource == null)
        {
            App.Logger.AddMessage("Starting discovery of services");
            _discoveryCancellationTokenSource = new();
            DiscoverServicesCommand.NotifyCanExecuteChanged();
            IReadOnlyList<IService> services = null;
            try
            {
                services = await _device.GetServicesAsync(_discoveryCancellationTokenSource.Token);
                App.Logger.AddMessage($"Discovered {services.Count} services");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Services = services;
                });

                _Characteristics = [];
                foreach(var service in Services)
                {
                    App.Logger.AddMessage($"Retrieving characteristics for service {service.Id}");
                    var chars = await service.GetCharacteristicsAsync();
                    _Characteristics.Add(new() {
                        Service = service,
                        Characteristics = chars.Select(c => new BLECharacteristicViewModel(c)).ToList().AsReadOnly()
                    });
                    App.Logger.AddMessage($"Received {chars.Count} characteristics");
                }
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Characteristics = _Characteristics.AsReadOnly();
                    RaisePropertyChanged(nameof(Characteristics));
                });
            }
            catch (Exception e)
            {
                App.Logger.AddMessage($"Discovery failed: {e}");
            }
            _discoveryCancellationTokenSource.Dispose();
            _discoveryCancellationTokenSource = null;
            DiscoverServicesCommand.NotifyCanExecuteChanged();
            App.Logger.AddMessage("Discovery... DONE");
        }
    }
    #endregion Services

    #region Characteristics
    public ICommand ReadCharacteristicValueCommand { get; init; }
    private async void ReadCharacteristicValue(object o)
    {
        if (o is not null)
        {
            ICharacteristic characteristic = (ICharacteristic)o;
            try
            {
                //characteristic.ValueUpdated += Characteristic_ValueUpdated;
                App.Logger.AddMessage($"Reading Characteristic {characteristic.Id}...");
                IReadOnlyList<IDescriptor> descriptors = await characteristic.GetDescriptorsAsync();
                var value = await characteristic.ReadAsync();
                //characteristic.ValueUpdated -= Characteristic_ValueUpdated;
                App.Logger.AddMessage($"Received value:\n{value.data.Length} bytes");
            }
            catch (Exception e)
            {
                App.Logger.AddMessage($"Reading characteristic failed: {e}");
            }
            App.Logger.AddMessage($"Done Reading Characteristic {characteristic.Id}.");
        }
    }

    private void Characteristic_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            App.Logger.AddMessage($">> ValueUpdated for {e.Characteristic.Id}");
        });
    }
    #endregion Characteristics

    #region Disconnect
    private void Adapter_DeviceConnectionLost(object sender, DeviceErrorEventArgs e)
    {
        App.Logger.AddMessage($"DeviceConnectionLost:\n{e.Device.Name}\n{e.Device.Id}\n{e.ErrorMessage}");
        HandleDisconnectedDevice(e.Device);
    }

    private void Adapter_DeviceDisconnected(object sender, DeviceEventArgs e)
    {
        App.Logger.AddMessage($"DeviceDisconnected:\n{e.Device.Name}\n{e.Device.Id}");
        HandleDisconnectedDevice(e.Device);
    }

    private void HandleDisconnectedDevice(IDevice _)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            RaisePropertyChanged(nameof(Connected));
            DiscoverServicesCommand.NotifyCanExecuteChanged();
        });
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
