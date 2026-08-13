using System.IO;
using System.Windows;
using WpfComboBox = System.Windows.Controls.ComboBox;

namespace VanguardVolume.App;

public partial class MainWindow : Window
{
    private readonly MixerController _controller;
    private readonly KeyBindingSettings _keyBindingSettings;
    private readonly Action<IReadOnlyDictionary<int, uint>> _applyMacroKeyMappings;
    private readonly Action<bool> _applyStartWithWindows;
    private readonly WpfComboBox[] _macroKeyBoxes;

    public MainWindow(
        MixerController controller,
        KeyBindingSettings keyBindingSettings,
        Action<IReadOnlyDictionary<int, uint>> applyMacroKeyMappings,
        Action<bool> applyStartWithWindows)
    {
        InitializeComponent();
        _controller = controller;
        _keyBindingSettings = keyBindingSettings;
        _applyMacroKeyMappings = applyMacroKeyMappings;
        _applyStartWithWindows = applyStartWithWindows;
        _macroKeyBoxes = [Macro1Key, Macro2Key, Macro3Key, Macro4Key, Macro5Key, Macro6Key];
        for (var slot = 1; slot <= _macroKeyBoxes.Length; slot++)
        {
            var comboBox = _macroKeyBoxes[slot - 1];
            comboBox.ItemsSource = KeyBindingSettings.SupportedKeys;
            comboBox.DisplayMemberPath = nameof(MacroKeyOption.DisplayName);
            comboBox.SelectedValuePath = nameof(MacroKeyOption.VirtualKey);
            comboBox.SelectedValue = _keyBindingSettings.MacroKeys[slot];
        }
        StartWithWindowsCheckBox.IsChecked = _keyBindingSettings.StartWithWindows;
        _controller.StateChanged += (_, _) => Dispatcher.Invoke(RefreshMapping);
        RefreshMapping();
    }

    public void RefreshMapping() => MappingText.Text = string.Join(
        Environment.NewLine,
        _controller.Assignments.Select(target => $"G{target.Slot}  {target.Name}  {target.VolumePercent}%{(target.IsMuted ? "  MUTE" : string.Empty)}"));

    private void SaveMacroKeys_Click(object sender, RoutedEventArgs e)
    {
        var macroKeys = _macroKeyBoxes
            .Select((comboBox, index) => new { Slot = index + 1, VirtualKey = (uint)comboBox.SelectedValue })
            .ToDictionary(binding => binding.Slot, binding => binding.VirtualKey);

        try
        {
            _applyMacroKeyMappings(macroKeys);
            BindingStatusText.Foreground = System.Windows.Media.Brushes.ForestGreen;
            BindingStatusText.Text = "Bindings saved and applied.";
        }
        catch (InvalidDataException exception)
        {
            BindingStatusText.Foreground = System.Windows.Media.Brushes.Firebrick;
            BindingStatusText.Text = exception.Message;
        }
        catch (IOException exception)
        {
            BindingStatusText.Foreground = System.Windows.Media.Brushes.Firebrick;
            BindingStatusText.Text = $"Could not save bindings: {exception.Message}";
        }
    }

    private void StartWithWindows_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _applyStartWithWindows(StartWithWindowsCheckBox.IsChecked == true);
            BindingStatusText.Foreground = System.Windows.Media.Brushes.ForestGreen;
            BindingStatusText.Text = "Startup preference saved.";
        }
        catch (IOException exception)
        {
            BindingStatusText.Foreground = System.Windows.Media.Brushes.Firebrick;
            BindingStatusText.Text = $"Could not save startup preference: {exception.Message}";
        }
        catch (UnauthorizedAccessException exception)
        {
            BindingStatusText.Foreground = System.Windows.Media.Brushes.Firebrick;
            BindingStatusText.Text = $"Could not change startup: {exception.Message}";
        }
    }
}