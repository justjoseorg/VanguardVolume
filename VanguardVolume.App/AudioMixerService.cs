using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System.Diagnostics;

namespace VanguardVolume.App;

public sealed class AudioMixerService : IDisposable
{
    private const string MasterId = "master";
    private readonly MMDeviceEnumerator _enumerator = new();
    private MMDevice? _device;
    private Dictionary<string, List<SimpleAudioVolume>> _volumesById = new(StringComparer.OrdinalIgnoreCase);

    public MixerTarget GetMasterTarget()
    {
        var device = GetDevice();
        return new MixerTarget(1, MasterId, "Master", device.AudioEndpointVolume.MasterVolumeLevelScalar,
            device.AudioEndpointVolume.Mute, true);
    }

    public IReadOnlyList<MixerTarget> GetApplicationTargets()
    {
        var device = GetDevice();
        var groups = new Dictionary<string, (string Name, List<SimpleAudioVolume> Volumes)>(StringComparer.OrdinalIgnoreCase);

        var sessions = device.AudioSessionManager.Sessions;
        for (var index = 0; index < sessions.Count; index++)
        {
            using var session = sessions[index];
            if (session.State == AudioSessionState.AudioSessionStateExpired)
            {
                continue;
            }

            if (session.GetProcessID == 0)
            {
                continue;
            }

            var id = GetApplicationId(session, out var name);
            if (!groups.TryGetValue(id, out var group))
            {
                group = (name, []);
                groups.Add(id, group);
            }

            group.Volumes.Add(session.SimpleAudioVolume);
            groups[id] = group;
        }

        _volumesById = groups.ToDictionary(pair => pair.Key, pair => pair.Value.Volumes, StringComparer.OrdinalIgnoreCase);
        return groups.Select(pair =>
        {
            var volumes = pair.Value.Volumes;
            return new MixerTarget(0, pair.Key, pair.Value.Name, volumes.Average(volume => volume.Volume),
                volumes.All(volume => volume.Mute), false);
        }).OrderBy(target => target.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public void SetVolume(string id, float volume)
    {
        if (id == MasterId)
        {
            GetDevice().AudioEndpointVolume.MasterVolumeLevelScalar = volume;
            return;
        }

        foreach (var sessionVolume in GetVolumes(id))
        {
            sessionVolume.Volume = volume;
        }
    }

    public void SetMute(string id, bool mute)
    {
        if (id == MasterId)
        {
            GetDevice().AudioEndpointVolume.Mute = mute;
            return;
        }

        foreach (var sessionVolume in GetVolumes(id))
        {
            sessionVolume.Mute = mute;
        }
    }

    public void Dispose()
    {
        _device?.Dispose();
        _enumerator.Dispose();
    }

    private MMDevice GetDevice()
    {
        _device ??= _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return _device;
    }

    private IReadOnlyList<SimpleAudioVolume> GetVolumes(string id) =>
        _volumesById.TryGetValue(id, out var volumes)
            ? volumes
            : throw new InvalidOperationException($"Audio application '{id}' is no longer available.");

    private static string GetApplicationId(AudioSessionControl session, out string name)
    {
        var processId = session.GetProcessID;
        if (processId == 0)
        {
            name = string.IsNullOrWhiteSpace(session.DisplayName) ? "System Sounds" : session.DisplayName;
            return $"system:{session.GetSessionIdentifier}";
        }

        try
        {
            using var process = Process.GetProcessById((int)processId);
            name = string.IsNullOrWhiteSpace(process.MainWindowTitle) ? process.ProcessName : process.MainWindowTitle;
            return $"process:{process.MainModule?.FileName ?? process.ProcessName}";
        }
        catch (ArgumentException)
        {
            name = string.IsNullOrWhiteSpace(session.DisplayName) ? $"Process {processId}" : session.DisplayName;
            return $"pid:{processId}";
        }
    }
}
