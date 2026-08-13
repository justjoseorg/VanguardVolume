using System.Runtime.InteropServices;

namespace VanguardVolume.App;

public enum GlobalKey
{
    Macro1, Macro2, Macro3, Macro4, Macro5, Macro6, VolumeUp, VolumeDown, VolumeMute
}

public sealed class KeyboardHook : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100;
    private const int WmSysKeyDown = 0x0104;
    private const uint VkVolumeMute = 0xAD;
    private const uint VkVolumeDown = 0xAE;
    private const uint VkVolumeUp = 0xAF;
    private readonly HookProc _callback;
    private readonly object _mappingLock = new();
    private Dictionary<uint, GlobalKey> _macroKeys = [];
    private nint _hook;

    public KeyboardHook(IReadOnlyDictionary<int, uint> macroKeys)
    {
        _callback = HookCallback;
        UpdateMacroKeys(macroKeys);
    }

    public event EventHandler<GlobalKey>? KeyPressed;

    public void UpdateMacroKeys(IReadOnlyDictionary<int, uint> macroKeys)
    {
        var updatedKeys = macroKeys.ToDictionary(pair => pair.Value, pair => (GlobalKey)(pair.Key - 1));
        lock (_mappingLock)
        {
            _macroKeys = updatedKeys;
        }
    }

    public void Start()
    {
        if (_hook != nint.Zero)
        {
            return;
        }

        _hook = SetWindowsHookEx(WhKeyboardLl, _callback, GetModuleHandle(null), 0);
        if (_hook == nint.Zero)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Unable to install keyboard hook.");
        }
    }

    public void Dispose()
    {
        if (_hook != nint.Zero)
        {
            UnhookWindowsHookEx(_hook);
            _hook = nint.Zero;
        }
    }

    private nint HookCallback(int code, nint wParam, nint lParam)
    {
        if (code >= 0 && ((int)wParam == WmKeyDown || (int)wParam == WmSysKeyDown))
        {
            var virtualKey = (uint)Marshal.ReadInt32(lParam);
            if (TryMapKey(virtualKey, out var key))
            {
                KeyPressed?.Invoke(this, key);
                return 1;
            }
        }

        return CallNextHookEx(_hook, code, wParam, lParam);
    }

    private bool TryMapKey(uint virtualKey, out GlobalKey key)
    {
        lock (_mappingLock)
        {
            if (_macroKeys.TryGetValue(virtualKey, out key))
            {
                return true;
            }
        }

        key = virtualKey switch
        {
            VkVolumeUp => GlobalKey.VolumeUp,
            VkVolumeDown => GlobalKey.VolumeDown,
            VkVolumeMute => GlobalKey.VolumeMute,
            _ => default
        };
        return virtualKey is VkVolumeUp or VkVolumeDown or VkVolumeMute;
    }

    private delegate nint HookProc(int code, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, HookProc callback, nint module, uint threadId);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("user32.dll")]
    private static extern nint CallNextHookEx(nint hook, int code, nint wParam, nint lParam);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern nint GetModuleHandle(string? moduleName);
}
