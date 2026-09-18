namespace VanguardVolume.App;

public sealed class MixerController
{
    private readonly AudioMixerService _audio;
    private readonly StableAssignmentStore _assignmentStore = new();
    private HashSet<string> _bannedApplicationIds = new(StringComparer.OrdinalIgnoreCase);
    private List<string> _priorityApplicationIds = [];
    private List<MixerTarget> _assignments = [];
    private List<MixerTarget> _availableApplications = [];
    private int? _selectedSlot;

    public MixerController(AudioMixerService audio) => _audio = audio;

    public event EventHandler? StateChanged;
    public IReadOnlyList<MixerTarget> Assignments => _assignments;
    public IReadOnlyList<MixerTarget> AvailableApplications => _availableApplications;
    public int? SelectedSlot => _selectedSlot;
    public bool HasSelectedTarget => _selectedSlot is not null && _assignments.Any(target => target.Slot == _selectedSlot);

    public void Refresh()
    {
        _availableApplications = _audio.GetApplicationTargets()
            .Where(target => !_bannedApplicationIds.Contains(target.Id))
            .ToList();
        _assignments = _assignmentStore.Assign(_availableApplications, _priorityApplicationIds).ToList();
        if (!HasSelectedTarget)
        {
            _selectedSlot = null;
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetBannedApplicationIds(IEnumerable<string> applicationIds)
    {
        _bannedApplicationIds = new HashSet<string>(applicationIds, StringComparer.OrdinalIgnoreCase);
        Refresh();
    }

    public void SetPriorityApplicationIds(IEnumerable<string> applicationIds)
    {
        _priorityApplicationIds = applicationIds
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        Refresh();
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
        var target = GetSelectedTargetOrDefault();
        if (target is null)
        {
            return;
        }

        _audio.SetVolume(target.Id, Math.Clamp(target.Volume + delta, 0f, 1f));
        Refresh();
    }

    public void ToggleSelectedMute()
    {
        var target = GetSelectedTargetOrDefault();
        if (target is null)
        {
            return;
        }

        _audio.SetMute(target.Id, !target.IsMuted);
        Refresh();
    }

    private MixerTarget? GetSelectedTargetOrDefault() =>
        _selectedSlot is null ? null : _assignments.FirstOrDefault(target => target.Slot == _selectedSlot);
}
