using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System.Diagnostics;

namespace VanguardVolume.App;

public sealed class AudioMixerService : IDisposable
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private Dictionary<string, List<SimpleAudioVolume>> _volumesById = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<MixerTarget> GetApplicationTargets()
    {
        var groups = new Dictionary<string, (string Name, List<SimpleAudioVolume> Volumes)>(StringComparer.OrdinalIgnoreCase);
        var devices = _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        foreach (var device in devices)
        {
            using (device)
            {
                var sessions = device.AudioSessionManager.Sessions;
                for (var index = 0; index < sessions.Count; index++)
                {
                    using var session = sessions[index];
                    if (session.State == AudioSessionState.AudioSessionStateExpired || session.GetProcessID == 0)
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
            }
        }

        _volumesById = groups.ToDictionary(pair => pair.Key, pair => pair.Value.Volumes, StringComparer.OrdinalIgnoreCase);
        return groups.Select(pair =>
        {
            var volumes = pair.Value.Volumes;
            return new MixerTarget(0, pair.Key, pair.Value.Name, volumes.Average(volume => volume.Volume),
                volumes.All(volume => volume.Mute));
        }).OrderBy(target => target.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public void SetVolume(string id, float volume)
    {
        foreach (var sessionVolume in GetVolumes(id))
        {
            sessionVolume.Volume = volume;
        }
    }

    public void SetMute(string id, bool mute)
    {
        foreach (var sessionVolume in GetVolumes(id))
        {
            sessionVolume.Mute = mute;
        }
    }

    public void Dispose()
    {
        _enumerator.Dispose();
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
