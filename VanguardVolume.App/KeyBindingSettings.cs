using System.IO;
using System.Text.Json;

namespace VanguardVolume.App;

public sealed class KeyBindingSettings
{
    private const uint FirstSupportedKey = 0x7C; // F13
    private const uint LastSupportedKey = 0x87; // F24
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VanguardVolume",
        "settings.json");

    public Dictionary<int, uint> MacroKeys { get; set; } = CreateDefaultMappings();
    public bool StartWithWindows { get; set; }

    public static IReadOnlyList<MacroKeyOption> SupportedKeys { get; } =
        Enumerable.Range((int)FirstSupportedKey, (int)(LastSupportedKey - FirstSupportedKey + 1))
            .Select(key => new MacroKeyOption((uint)key, $"F{key - 0x6F}"))
            .ToList();

    public static KeyBindingSettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            return new KeyBindingSettings();
        }

        var settings = JsonSerializer.Deserialize<KeyBindingSettings>(File.ReadAllText(SettingsPath))
            ?? throw new InvalidDataException("Vanguard Volume settings are empty.");
        settings.Validate();
        return settings;
    }

    public void Save()
    {
        Validate();
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void Update(IReadOnlyDictionary<int, uint> macroKeys)
    {
        MacroKeys = macroKeys.ToDictionary(pair => pair.Key, pair => pair.Value);
        Validate();
    }

    private void Validate()
    {
        if (MacroKeys.Count != 6 || Enumerable.Range(1, 6).Except(MacroKeys.Keys).Any())
        {
            throw new InvalidDataException("Configure exactly one binding for each macro key G1-G6.");
        }

        if (MacroKeys.Values.Any(key => key is < FirstSupportedKey or > LastSupportedKey)
            || MacroKeys.Values.Distinct().Count() != MacroKeys.Count)
        {
            throw new InvalidDataException("Macro bindings must be unique keys from F13 through F24.");
        }
    }

    private static Dictionary<int, uint> CreateDefaultMappings() =>
        Enumerable.Range(1, 6).ToDictionary(slot => slot, slot => FirstSupportedKey + (uint)(slot - 1));
}

public sealed record MacroKeyOption(uint VirtualKey, string DisplayName);
