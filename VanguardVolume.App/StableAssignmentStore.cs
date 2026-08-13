namespace VanguardVolume.App;

/// <summary>Maintains fixed macro-key placements while applications come and go.</summary>
public sealed class StableAssignmentStore
{
    private readonly Dictionary<int, string> _idsBySlot = new();

    public IReadOnlyList<MixerTarget> Assign(
        IReadOnlyList<MixerTarget> available,
        IReadOnlyList<string>? priorityIds = null)
    {
        var byId = available.ToDictionary(target => target.Id, StringComparer.OrdinalIgnoreCase);
        var priorities = (priorityIds ?? [])
            .Where(byId.ContainsKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();

        foreach (var slot in Enumerable.Range(2, priorities.Count))
        {
            _idsBySlot.Remove(slot);
        }
        foreach (var priority in priorities)
        {
            foreach (var slot in _idsBySlot.Where(pair => string.Equals(pair.Value, priority, StringComparison.OrdinalIgnoreCase)).Select(pair => pair.Key).ToArray())
            {
                _idsBySlot.Remove(slot);
            }
        }
        for (var index = 0; index < priorities.Count; index++)
        {
            _idsBySlot[index + 2] = priorities[index];
        }

        foreach (var stale in _idsBySlot.Where(pair => !byId.ContainsKey(pair.Value)).Select(pair => pair.Key).ToArray())
        {
            _idsBySlot.Remove(stale);
        }

        var unassigned = available
            .Where(target => !_idsBySlot.Values.Contains(target.Id, StringComparer.OrdinalIgnoreCase))
            .ToList();
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
