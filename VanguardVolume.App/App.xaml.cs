using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace VanguardVolume.App;

public partial class App : System.Windows.Application
{
    private AudioMixerService? _audio;
    private MixerController? _controller;
    private KeyboardHook? _keyboardHook;
    private MainWindow? _mainWindow;
    private System.Windows.Forms.NotifyIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _audio = new AudioMixerService();
        _controller = new MixerController(_audio);
        _keyboardHook = new KeyboardHook();
        _keyboardHook.KeyPressed += OnGlobalKeyPressed;
        _keyboardHook.Start();

        _mainWindow = new MainWindow(_controller);
        MainWindow = _mainWindow;
        _trayIcon = CreateTrayIcon();
        _controller.Refresh();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _keyboardHook?.Dispose();
        _trayIcon?.Dispose();
        _audio?.Dispose();
        base.OnExit(e);
    }

    private void OnGlobalKeyPressed(object? sender, GlobalKey key)
    {
        Dispatcher.BeginInvoke(() =>
        {
            switch (key)
            {
                case GlobalKey.Macro1: _controller!.SelectSlot(1); break;
                case GlobalKey.Macro2: _controller!.SelectSlot(2); break;
                case GlobalKey.Macro3: _controller!.SelectSlot(3); break;
                case GlobalKey.Macro4: _controller!.SelectSlot(4); break;
                case GlobalKey.Macro5: _controller!.SelectSlot(5); break;
                case GlobalKey.Macro6: _controller!.SelectSlot(6); break;
                case GlobalKey.VolumeUp: _controller!.AdjustSelectedVolume(0.02f); break;
                case GlobalKey.VolumeDown: _controller!.AdjustSelectedVolume(-0.02f); break;
                case GlobalKey.VolumeMute: _controller!.ToggleSelectedMute(); break;
            }

            if (key is >= GlobalKey.Macro1 and <= GlobalKey.Macro6)
            {
                OpenWindowsVolumeMixer();
            }
        });
    }

    private void OpenWindowsVolumeMixer()
    {
        const byte vkControl = 0x11;
        const byte vkLeftWindows = 0x5B;
        const byte vkV = 0x56;

        KeyDown(vkLeftWindows);
        KeyDown(vkControl);
        PressKey(vkV);
        KeyUp(vkControl);
        KeyUp(vkLeftWindows);

        var scrollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        scrollTimer.Tick += (_, _) =>
        {
            scrollTimer.Stop();
            PressKey(0x22); // VK_NEXT scrolls the newly opened flyout to app volume controls.
        };
        scrollTimer.Start();
    }

    private System.Windows.Forms.NotifyIcon CreateTrayIcon()
    {
        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Show mapping", null, (_, _) => _mainWindow!.Show());
        menu.Items.Add("Refresh audio sessions", null, (_, _) => _controller!.Refresh());
        menu.Items.Add("Exit", null, (_, _) => Shutdown());

        return new System.Windows.Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "Vanguard Volume",
            Visible = true,
            ContextMenuStrip = menu
        };
    }

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, nuint extraInfo);

    private static void PressKey(byte virtualKey)
    {
        KeyDown(virtualKey);
        KeyUp(virtualKey);
    }

    private static void KeyDown(byte virtualKey) => keybd_event(virtualKey, 0, 0, 0);
    private static void KeyUp(byte virtualKey) => keybd_event(virtualKey, 0, 0x0002, 0);
}
