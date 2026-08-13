using System.Windows;

namespace VanguardVolume.App;

public partial class MainWindow : Window
{
    private readonly MixerController _controller;

    public MainWindow(MixerController controller)
    {
        InitializeComponent();
        _controller = controller;
        _controller.StateChanged += (_, _) => Dispatcher.Invoke(RefreshMapping);
        RefreshMapping();
    }

    public void RefreshMapping() => MappingText.Text = string.Join(
        Environment.NewLine,
        _controller.Assignments.Select(target => $"G{target.Slot}  {target.Name}  {target.VolumePercent}%{(target.IsMuted ? "  MUTE" : string.Empty)}"));
}