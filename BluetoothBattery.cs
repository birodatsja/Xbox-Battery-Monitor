using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace XboxBatteryMonitor;

/// <summary>
/// Reads the exact battery percentage of paired Xbox controllers over Bluetooth LE
/// using the standard GATT Battery Service.
/// </summary>
internal static class BluetoothBattery
{
    public static async Task<List<ControllerVm>> ReadAsync()
    {
        var result = new List<ControllerVm>();
        try
        {
            var selector = BluetoothLEDevice.GetDeviceSelectorFromPairingState(true);
            var devices = await DeviceInformation.FindAllAsync(selector);

            foreach (var info in devices)
            {
                if (!info.Name.Contains("Xbox", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    var vm = await ReadDeviceAsync(info);
                    if (vm != null) result.Add(vm);
                }
                catch
                {
                    // Skip this device and try the next one
                }
            }
        }
        catch
        {
            // Bluetooth off or unavailable: the caller falls back to XInput
        }
        return result;
    }

    private static async Task<ControllerVm?> ReadDeviceAsync(DeviceInformation info)
    {
        using var device = await BluetoothLEDevice.FromIdAsync(info.Id);
        if (device == null || device.ConnectionStatus != BluetoothConnectionStatus.Connected)
            return null;

        var services = await device.GetGattServicesForUuidAsync(GattServiceUuids.Battery, BluetoothCacheMode.Uncached);
        if (services.Status != GattCommunicationStatus.Success)
            return null;

        foreach (var service in services.Services)
        {
            var chars = await service.GetCharacteristicsForUuidAsync(GattCharacteristicUuids.BatteryLevel, BluetoothCacheMode.Uncached);
            if (chars.Status != GattCommunicationStatus.Success)
                continue;

            foreach (var characteristic in chars.Characteristics)
            {
                var read = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                if (read.Status != GattCommunicationStatus.Success)
                    continue;

                int level = Math.Clamp((int)DataReader.FromBuffer(read.Value).ReadByte(), 0, 100);
                var name = string.IsNullOrWhiteSpace(info.Name) ? "Xbox Controller" : info.Name;
                return new ControllerVm(info.Id, name, $"{level}%", level, Levels.BrushFor(level),
                    "Connected via Bluetooth", true, level <= 20);
            }
        }
        return null;
    }
}
