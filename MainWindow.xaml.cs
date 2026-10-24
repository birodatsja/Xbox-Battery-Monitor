using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace XboxBatteryMonitor;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(5) };
    private readonly Tray _tray;
    private bool _busy;
    private bool _exiting;

    public MainWindow()
    {
        InitializeComponent();

        _tray = new Tray(ShowWindow, ExitApp);
        StartupBox.IsChecked = Startup.IsEnabled;
        _tray.StartupToggled += on => StartupBox.IsChecked = on;

        _timer.Tick += async (_, _) => await RefreshAsync();
        _timer.Start();
        _ = RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // Bluetooth gives an exact percentage; fall back to XInput (4-level) otherwise
            var controllers = await BluetoothBattery.ReadAsync();
            if (controllers.Count == 0)
            {
                for (uint i = 0; i < 4; i++)
                {
                    var vm = XInput.Read(i);
                    if (vm != null) controllers.Add(vm);
                }
            }

            ControllerList.ItemsSource = controllers;
            EmptyText.Visibility = controllers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            StatusText.Text = $"Updated {DateTime.Now:T} (refreshes every 5 seconds)";
            _tray.Update(controllers);
        }
        finally
        {
            _busy = false;
        }
    }

    private void StartupBox_Click(object sender, RoutedEventArgs e)
    {
        _tray.StartupChecked = StartupBox.IsChecked == true;
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApp()
    {
        _exiting = true;
        _timer.Stop();
        _tray.Dispose();
        Application.Current.Shutdown();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized) Hide();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_exiting)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        base.OnClosing(e);
    }
}
