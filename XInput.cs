using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace XboxBatteryMonitor;

internal static class XInput
{
    private const string Dll = "xinput1_4.dll";
    private const byte DevTypeGamepad = 0x00;

    private const byte BatteryDisconnected = 0x00;
    private const byte BatteryWired = 0x01;
    private const byte BatteryAlkaline = 0x02;
    private const byte BatteryNiMh = 0x03;
    private const byte BatteryUnknown = 0xFF;

    [StructLayout(LayoutKind.Sequential)]
    private struct BatteryInformation
    {
        public byte BatteryType;
        public byte BatteryLevel; // 0 = Empty, 1 = Low, 2 = Medium, 3 = Full
    }

    [DllImport(Dll, EntryPoint = "XInputGetBatteryInformation")]
    private static extern uint GetBatteryInformation(uint userIndex, byte devType, out BatteryInformation info);

    /// <summary>Reads one XInput slot (0-3). Returns null if nothing is connected there.</summary>
    public static ControllerVm? Read(uint index)
    {
        try
        {
            if (GetBatteryInformation(index, DevTypeGamepad, out var b) != 0 || b.BatteryType == BatteryDisconnected)
                return null;

            var id = $"xinput{index}";
            var name = $"Controller {index + 1}";

            if (b.BatteryType == BatteryWired)
                return new(id, name, "Wired", 100, Brushes.LimeGreen, "Plugged in, no battery reported", true, false);

            if (b.BatteryType == BatteryUnknown)
                return new(id, name, "Unknown", 0, Brushes.Gray, "Windows can't read the battery through XInput for this connection", false, false);

            var source = b.BatteryType switch
            {
                BatteryAlkaline => "AA batteries",
                BatteryNiMh => "Rechargeable pack",
                _ => "Battery"
            };

            return b.BatteryLevel switch
            {
                0 => new(id, name, "Empty", 5, Brushes.Red, $"{source} - charge or replace now", true, true),
                1 => new(id, name, "Low", 25, Brushes.Orange, $"{source} - running low", true, true),
                2 => new(id, name, "Medium", 60, Brushes.Gold, source, true, false),
                _ => new(id, name, "Full", 100, Brushes.LimeGreen, source, true, false),
            };
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            return null;
        }
    }
}
