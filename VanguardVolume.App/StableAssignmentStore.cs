namespace VanguardVolume.App;

/// <summary>Maintains fixed macro-key placements while applications come and go.</summary>
public sealed class StableAssignmentStore
{
    private readonly Dictionary<int, string> _idsBySlot = new();

    public IReadOnlyList<MixerTarget> Assign(IReadOnlyList<MixerTarget> available)
    {
        var byId = available.ToDictionary(target => target.Id, StringComparer.OrdinalIgnoreCase);
        foreach (var stale in _idsBySlot.Where(pair => !byId.ContainsKey(pair.Value)).Select(pair => pair.Key).ToArray())
        {
            _idsBySlot.Remove(stale);
        }

        var unassigned = available.Where(target => !_idsBySlot.ContainsValue(target.Id)).ToList();
        for (var slot = 2; slot <= 6 && unassigned.Count > 0; slot++)
        {
            if (_idsBySlot.ContainsKey(slot))
            {
                continue;
            }

            _idsBySlot[slot] = unassigned[0].Id;
            unassigned.RemoveAt(0);
        }

        return _idsBySlot.OrderBy(pair => pair.Key).Select(pair => byId[pair.Value] with { Slot = pair.Key }).ToList();
    }
}
