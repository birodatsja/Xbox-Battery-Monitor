using System;
using Microsoft.Win32;

namespace XboxBatteryMonitor;

internal static class Startup
{
    private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "XboxBatteryMonitor";

    public static bool IsEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
                return key?.GetValue(ValueName) != null;
            }
            catch { return false; }
        }
    }

    public static void Set(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath, true) ?? Registry.CurrentUser.CreateSubKey(KeyPath);
            if (enable)
                key.SetValue(ValueName, $"\"{Environment.ProcessPath}\" --minimized");
            else
                key.DeleteValue(ValueName, false);
        }
        catch
        {
            // Registry not writable: ignore
        }
    }
}
