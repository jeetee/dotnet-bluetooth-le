using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace BLE.Client.Maui.ViewModels;

public class BLECharacteristicViewModel : BaseViewModel, ICharacteristic
{
    private readonly ICharacteristic _characteristic;

    public BLECharacteristicViewModel(ICharacteristic characteristic)
    {
        _characteristic = characteristic;
    }

    #region ICharacteristic - transient properties
    public Guid Id => _characteristic.Id;
    public string Uuid => _characteristic.Uuid;
    public string Name => _characteristic.Name;
    public byte[] Value => _characteristic.Value;
    public string StringValue => _characteristic.StringValue;
    public CharacteristicPropertyType Properties => _characteristic.Properties;
    public CharacteristicWriteType WriteType
    { 
        get => _characteristic.WriteType;
        set
        {
            if (_characteristic.WriteType != value)
            {
                _characteristic.WriteType = value;
                RaisePropertyChanged();
            }
        }
    }
    public bool CanRead => _characteristic.CanRead;
    public bool CanWrite => _characteristic.CanWrite;
    public bool CanUpdate => _characteristic.CanUpdate;
    public IService Service => _characteristic.Service;
    #endregion ICharacteristic - transient properties

    #region Derived Properties
    public int ValueByteCount => _characteristic.Value.Length;
    public string ValueAsHex
    {
        get
        {
            return BitConverter.ToString(Value).Replace('-', ' ');
        }
    }
    #endregion Derived Properties

    public event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated
    {
        add
        {
            _characteristic.ValueUpdated += value;
        }

        remove
        {
            _characteristic.ValueUpdated -= value;
        }
    }

    #region ICharacteristic - transient calls
    public Task<IDescriptor> GetDescriptorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _characteristic.GetDescriptorAsync(id, cancellationToken);
    }
    public Task<IReadOnlyList<IDescriptor>> GetDescriptorsAsync(CancellationToken cancellationToken = default)
    {
        return _characteristic.GetDescriptorsAsync(cancellationToken);
    }
    public async Task<(byte[] data, int resultCode)> ReadAsync(CancellationToken cancellationToken = default)
    {
        byte[] oldValue = Value;
        var readresult = await _characteristic.ReadAsync(cancellationToken);
        if (Value != oldValue)
        {
            Characteristic_ValueUpdated(null, null);
        }
        return readresult;
    }
    public Task StartUpdatesAsync(CancellationToken cancellationToken = default)
    {
        _characteristic.ValueUpdated += Characteristic_ValueUpdated;
        return _characteristic.StartUpdatesAsync(cancellationToken);
    }
    public Task StopUpdatesAsync(CancellationToken cancellationToken = default)
    {
        _characteristic.ValueUpdated -= Characteristic_ValueUpdated;
        return _characteristic.StopUpdatesAsync(cancellationToken);
    }
    private void Characteristic_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
    {
        if (sender is not null)
        {
            App.Logger.AddMessage($"Value Updated for {Id}:\n{ValueAsHex}");
        }
        RaisePropertyChanged(nameof(Value));
        RaisePropertyChanged(nameof(ValueByteCount));
        RaisePropertyChanged(nameof(ValueAsHex));
        RaisePropertyChanged(nameof(StringValue));
    }

    public Task<int> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        return _characteristic.WriteAsync(data, cancellationToken);
    }
    #endregion ICharacteristic - transient calls

}
