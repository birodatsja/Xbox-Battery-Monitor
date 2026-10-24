using System.Windows.Media;

namespace XboxBatteryMonitor;

public record ControllerVm(
    string Id,
    string Name,
    string LevelText,
    double Percent,
    Brush Brush,
    string Detail,
    bool Known,
    bool IsLow);

internal static class Levels
{
    public static Brush BrushFor(double percent) =>
        percent <= 10 ? Brushes.Red :
        percent <= 30 ? Brushes.Orange :
        percent <= 70 ? Brushes.Gold :
        Brushes.LimeGreen;
}
