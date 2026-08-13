using System.IO;
using System.Windows;

namespace VanguardVolume.App;

public partial class App : System.Windows.Application
{
    private AudioMixerService? _audio;
    private MixerController? _controller;
    private KeyboardHook? _keyboardHook;
    private KeyBindingSettings? _keyBindingSettings;
    private MixerFlyout? _flyout;
    private MainWindow? _mainWindow;
    private System.Windows.Forms.NotifyIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _audio = new AudioMixerService();
        _controller = new MixerController(_audio);
        _keyBindingSettings = KeyBindingSettings.Load();
        _controller.SetBannedApplicationIds(_keyBindingSettings.BannedApplicationIds);
        _keyboardHook = new KeyboardHook(_keyBindingSettings.MacroKeys);
        _keyboardHook.KeyPressed += OnGlobalKeyPressed;
        _keyboardHook.Start();

        _flyout = new MixerFlyout(_controller);
        _trayIcon = CreateTrayIcon();
        if (e.Args.Contains("--settings", StringComparer.OrdinalIgnoreCase))
        {
            ShowSettings();
        }

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

            if (key is >= GlobalKey.Macro1 and <= GlobalKey.Macro6
                or GlobalKey.VolumeUp or GlobalKey.VolumeDown or GlobalKey.VolumeMute)
            {
                _flyout!.ShowForInteraction();
            }
        });
    }

    private void ApplyMacroKeyMappings(IReadOnlyDictionary<int, uint> macroKeys)
    {
        _keyBindingSettings!.Update(macroKeys);
        _keyBindingSettings.Save();
        _keyboardHook!.UpdateMacroKeys(_keyBindingSettings.MacroKeys);
    }

    private void ApplyStartWithWindows(bool enabled)
    {
        AutostartService.SetEnabled(enabled);
        _keyBindingSettings!.StartWithWindows = enabled;
        _keyBindingSettings.Save();
    }

    private void ApplyBannedApplicationIds(IReadOnlyCollection<string> applicationIds)
    {
        _keyBindingSettings!.BannedApplicationIds = new HashSet<string>(applicationIds, StringComparer.OrdinalIgnoreCase);
        _keyBindingSettings.Save();
        _controller!.SetBannedApplicationIds(_keyBindingSettings.BannedApplicationIds);
    }

    private void ShowSettings()
    {
        if (_mainWindow is null)
        {
            _mainWindow = new MainWindow(
                _controller!,
                _keyBindingSettings!,
                ApplyMacroKeyMappings,
                ApplyStartWithWindows,
                ApplyBannedApplicationIds);
            _mainWindow.Closed += (_, _) =>
            {
                _mainWindow = null;
                MainWindow = null;
            };
            MainWindow = _mainWindow;
        }

        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private System.Windows.Forms.NotifyIcon CreateTrayIcon()
    {
        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Open settings", null, (_, _) => Dispatcher.Invoke(ShowSettings));
        menu.Items.Add("Refresh audio sessions", null, (_, _) => Dispatcher.Invoke(() => _controller!.Refresh()));
        menu.Items.Add("Exit", null, (_, _) => Dispatcher.Invoke(Shutdown));

        return new System.Windows.Forms.NotifyIcon
        {
            Icon = new System.Drawing.Icon(Path.Combine(AppContext.BaseDirectory, "Assets", "vanguard-volume.ico")),
            Text = "Vanguard Volume",
            Visible = true,
            ContextMenuStrip = menu
        };
    }

}
