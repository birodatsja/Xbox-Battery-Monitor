using System.Linq;
using System.Threading;
using System.Windows;

namespace XboxBatteryMonitor;

public partial class App : Application
{
    private Mutex? _mutex;
    private bool _ownsMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Only allow one copy at a time
        _mutex = new Mutex(true, "XboxBatteryMonitor.SingleInstance", out _ownsMutex);
        if (!_ownsMutex)
        {
            Shutdown();
            return;
        }

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        base.OnStartup(e);

        var window = new MainWindow();
        if (!e.Args.Contains("--minimized"))
            window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_ownsMutex) _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
