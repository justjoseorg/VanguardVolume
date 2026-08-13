using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;
using MediaFontFamily = System.Windows.Media.FontFamily;

namespace VanguardVolume.App;

public sealed class MixerFlyout : Window
{
    private readonly MixerController _controller;
    private readonly StackPanel _rows = new();
    private readonly DispatcherTimer _hideTimer;

    public MixerFlyout(MixerController controller)
    {
        _controller = controller;
        Width = 370;
        Topmost = true;
        ShowActivated = false;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        Background = new SolidColorBrush(MediaColor.FromRgb(28, 30, 35));
        Content = _rows;
        _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _hideTimer.Tick += (_, _) => Hide();
    }

    public void ShowTemporarily()
    {
        Refresh();
        if (!IsVisible)
        {
            Show();
        }

        _hideTimer.Stop();
        _hideTimer.Start();
    }

    public void Refresh()
    {
        _rows.Children.Clear();
        foreach (var target in _controller.Assignments)
        {
            var selected = target.Slot == _controller.SelectedSlot;
            _rows.Children.Add(new Border
            {
                Margin = new Thickness(6, 3, 6, 3),
                Padding = new Thickness(10, 7, 10, 7),
                CornerRadius = new CornerRadius(3),
                Background = new SolidColorBrush(selected ? MediaColor.FromRgb(0, 123, 180) : MediaColor.FromRgb(48, 51, 59)),
                Child = new TextBlock
                {
                    Foreground = MediaBrushes.White,
                    FontFamily = new MediaFontFamily("Consolas"),
                    Text = $"G{target.Slot}  {target.Name,-22} {(target.IsMuted ? "MUTE" : $"{target.VolumePercent}%")}"
                }
            });
        }
    }
}
