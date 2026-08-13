namespace VanguardVolume.App;

public sealed record MixerTarget(int Slot, string Id, string Name, float Volume, bool IsMuted, bool IsMaster)
{
    public int VolumePercent => (int)Math.Round(Volume * 100);
}
