namespace VanguardVolume.App;

public sealed record MixerTarget(int Slot, string Id, string Name, float Volume, bool IsMuted)
{
    public int VolumePercent => (int)Math.Round(Volume * 100);
}
