using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace XboxBatteryMonitor;

internal sealed class Tray : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly ToolStripMenuItem _startupItem;
    private readonly HashSet<string> _warned = new();
    private Icon? _current;

    public event Action<bool>? StartupToggled;

    public bool StartupChecked
    {
        get => _startupItem.Checked;
        set => _startupItem.Checked = value;
    }

    public Tray(Action onOpen, Action onExit)
    {
        _startupItem = new ToolStripMenuItem("Start with Windows")
        {
            CheckOnClick = true,
            Checked = Startup.IsEnabled
        };
        _startupItem.CheckedChanged += (_, _) =>
        {
            Startup.Set(_startupItem.Checked);
            StartupToggled?.Invoke(_startupItem.Checked);
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => onOpen());
        menu.Items.Add(_startupItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => onExit());

        _icon = new NotifyIcon
        {
            ContextMenuStrip = menu,
            Text = "Xbox controller battery",
            Visible = true
        };
        _icon.DoubleClick += (_, _) => onOpen();
        SetIcon(-1);
    }

    public void Update(IReadOnlyList<ControllerVm> controllers)
    {
        var lowest = controllers.Where(c => c.Known).OrderBy(c => c.Percent).FirstOrDefault();

        if (lowest != null)
        {
            SetIcon(lowest.Percent);
            var text = controllers.Count == 1
                ? $"{lowest.Name}: {lowest.LevelText}"
                : string.Join(", ", controllers.Select(c => $"{c.Name}: {c.LevelText}"));
            _icon.Text = text.Length > 63 ? text[..63] : text;
        }
        else
        {
            SetIcon(-1);
            _icon.Text = controllers.Count == 0 ? "Xbox controller: not connected" : "Xbox controller: battery unknown";
        }

        // Low battery notification, once per controller until it recovers or disconnects
        foreach (var c in controllers)
        {
            if (c.IsLow)
            {
                if (_warned.Add(c.Id))
                    _icon.ShowBalloonTip(6000, "Controller battery low",
                        $"{c.Name} is at {c.LevelText}. Time to charge it.", ToolTipIcon.Warning);
            }
            else if (c.Percent > 30)
            {
                _warned.Remove(c.Id);
            }
        }
        _warned.RemoveWhere(id => controllers.All(c => c.Id != id));
    }

    private void SetIcon(double percent)
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            using var outline = new Pen(Color.Silver, 2);
            g.DrawRectangle(outline, 3, 8, 24, 16);
            using var nub = new SolidBrush(Color.Silver);
            g.FillRectangle(nub, 27, 12, 3, 8);

            if (percent >= 0)
            {
                var color = percent <= 10 ? Color.Red
                          : percent <= 30 ? Color.Orange
                          : percent <= 70 ? Color.Gold
                          : Color.LimeGreen;
                int width = (int)Math.Max(2, 20 * percent / 100.0);
                using var fill = new SolidBrush(color);
                g.FillRectangle(fill, 5, 10, width, 12);
            }
        }

        var handle = bmp.GetHicon();
        var icon = (Icon)Icon.FromHandle(handle).Clone();
        DestroyIcon(handle);

        _icon.Icon = icon;
        _current?.Dispose();
        _current = icon;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
        _current?.Dispose();
    }
}
