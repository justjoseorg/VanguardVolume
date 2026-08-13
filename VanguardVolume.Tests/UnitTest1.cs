using VanguardVolume.App;

namespace VanguardVolume.Tests;

public class StableAssignmentStoreTests
{
    [Fact]
    public void ExistingApplicationsKeepTheirMacroKey()
    {
        var store = new StableAssignmentStore();
        var first = store.Assign([
            new MixerTarget(0, "a", "Alpha", 0.5f, false, false),
            new MixerTarget(0, "b", "Bravo", 0.5f, false, false)
        ]);
        var refreshed = store.Assign([
            new MixerTarget(0, "b", "Bravo", 0.5f, false, false),
            new MixerTarget(0, "a", "Alpha", 0.5f, false, false),
            new MixerTarget(0, "c", "Charlie", 0.5f, false, false)
        ]);

        Assert.Equal(first.Single(target => target.Id == "a").Slot, refreshed.Single(target => target.Id == "a").Slot);
        Assert.Equal(first.Single(target => target.Id == "b").Slot, refreshed.Single(target => target.Id == "b").Slot);
        Assert.Equal(4, refreshed.Single(target => target.Id == "c").Slot);
    }
}
