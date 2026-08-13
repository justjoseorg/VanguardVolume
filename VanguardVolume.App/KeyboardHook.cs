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
    private const uint VkF13 = 0x7C;
    private const uint VkVolumeMute = 0xAD;
    private const uint VkVolumeDown = 0xAE;
    private const uint VkVolumeUp = 0xAF;
    private readonly HookProc _callback;
    private nint _hook;

    public KeyboardHook() => _callback = HookCallback;
    public event EventHandler<GlobalKey>? KeyPressed;

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

    private static bool TryMapKey(uint virtualKey, out GlobalKey key)
    {
        key = virtualKey switch
        {
            >= VkF13 and <= VkF13 + 5 => (GlobalKey)(virtualKey - VkF13),
            VkVolumeUp => GlobalKey.VolumeUp,
            VkVolumeDown => GlobalKey.VolumeDown,
            VkVolumeMute => GlobalKey.VolumeMute,
            _ => default
        };

        return virtualKey is >= VkF13 and <= VkF13 + 5 or VkVolumeUp or VkVolumeDown or VkVolumeMute;
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
