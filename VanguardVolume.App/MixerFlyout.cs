using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace VanguardVolume.App;

public sealed class MixerFlyout : Window
{
    private const int GwlExStyle = -20;
    private const nint WsExNoActivate = 0x08000000;
    private const nint WsExToolWindow = 0x00000080;
    private readonly MixerController _controller;
    private readonly StackPanel _rows = new();
    private readonly DispatcherTimer _hideTimer;

    public MixerFlyout(MixerController controller)
    {
        _controller = controller;
        Width = 390;
        Topmost = true;
        ShowActivated = false;
        ShowInTaskbar = false;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.Height;
        Background = new SolidColorBrush(Color.FromRgb(23, 28, 38));
        Content = new Border
        {
            Padding = new Thickness(8),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0, 174, 239)),
            BorderThickness = new Thickness(1),
            Child = _rows
        };
        _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _hideTimer.Tick += (_, _) =>
        {
            _hideTimer.Stop();
            Hide();
        };
    }

    public void ShowForInteraction()
    {
        Refresh();
        if (!IsVisible)
        {
            Show();
        }

        UpdateLayout();
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - ActualWidth - 16;
        Top = workArea.Bottom - ActualHeight - 16;
        _hideTimer.Stop();
        _hideTimer.Start();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var handle = new WindowInteropHelper(this).Handle;
        SetWindowLongPtr(handle, GwlExStyle, GetWindowLongPtr(handle, GwlExStyle) | WsExNoActivate | WsExToolWindow);
    }

    private void Refresh()
    {
        _rows.Children.Clear();
        foreach (var target in _controller.Assignments)
        {
            var selected = target.Slot == _controller.SelectedSlot;
            _rows.Children.Add(new Border
            {
                Margin = new Thickness(0, 3, 0, 3),
                Padding = new Thickness(10, 7, 10, 7),
                CornerRadius = new CornerRadius(3),
                Background = new SolidColorBrush(selected ? Color.FromRgb(0, 112, 160) : Color.FromRgb(48, 54, 67)),
                Child = new TextBlock
                {
                    Foreground = Brushes.White,
                    FontFamily = new FontFamily("Consolas"),
                    Text = $"G{target.Slot}  {target.Name,-22} {(target.IsMuted ? "MUTE" : $"{target.VolumePercent}%")}"
                }
            });
        }
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(nint windowHandle, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern nint SetWindowLongPtr(nint windowHandle, int index, nint newLong);
}
