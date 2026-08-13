namespace VanguardVolume.App;

public sealed class MixerController
{
    private readonly AudioMixerService _audio;
    private readonly StableAssignmentStore _assignmentStore = new();
    private List<MixerTarget> _assignments = [];
    private int _selectedSlot = 1;

    public MixerController(AudioMixerService audio) => _audio = audio;

    public event EventHandler? StateChanged;
    public IReadOnlyList<MixerTarget> Assignments => _assignments;
    public int SelectedSlot => _selectedSlot;

    public void Refresh()
    {
        var master = _audio.GetMasterTarget() with { Slot = 1 };
        _assignments = [master, .. _assignmentStore.Assign(_audio.GetApplicationTargets())];
        if (!_assignments.Any(target => target.Slot == _selectedSlot))
        {
            _selectedSlot = 1;
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SelectSlot(int slot)
    {
        Refresh();
        if (_assignments.Any(target => target.Slot == slot))
        {
            _selectedSlot = slot;
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AdjustSelectedVolume(float delta)
    {
        var target = GetSelectedTarget();
        _audio.SetVolume(target.Id, Math.Clamp(target.Volume + delta, 0f, 1f));
        Refresh();
    }

    public void ToggleSelectedMute()
    {
        var target = GetSelectedTarget();
        _audio.SetMute(target.Id, !target.IsMuted);
        Refresh();
    }

    private MixerTarget GetSelectedTarget() =>
        _assignments.FirstOrDefault(target => target.Slot == _selectedSlot) ?? _assignments.First(target => target.IsMaster);
}
