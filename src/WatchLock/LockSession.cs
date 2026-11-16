using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.Win32;
using static WatchLock.NativeMethods;

namespace WatchLock;

/// <summary>
/// One lock session: opens a see-through window on every monitor, installs keyboard/mouse hooks,
/// keeps the PC awake and falls back to the Windows lock on escapes like Ctrl+Alt+Del.
/// </summary>
public sealed class LockSession
{
    private readonly List<LockWindow> _windows = new();
    private readonly DispatcherTimer _timer;
    private readonly DateTime _startedAt = DateTime.Now;
    private readonly TimeSpan? _testDuration;
    private readonly Action<string> _onEnded;

    // Hook delegates are kept in fields so the GC doesn't collect them.
    private readonly HookProc _keyboardProc, _mouseProc;
    private IntPtr _keyboardHook, _mouseHook;

    private bool _active;
    private int _wrongAttempts;
    private bool _windowsLockRequested, _sessionWasLocked;
    private int _windowsLockAttempts;
    private DateTime _lastWindowsLockAttempt;

    public LockSession(TimeSpan? testDuration, Action<string> onEnded)
    {
        _testDuration = testDuration;
        _onEnded = onEnded;
        _keyboardProc = KeyboardHook;
        _mouseProc = MouseHook;
        _timer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += (_, _) => Check();
    }

    private LockWindow? Primary => _windows.FirstOrDefault(w => w.IsPrimary) ?? _windows.FirstOrDefault();

    public void Start()
    {
        _active = true;
        var monitors = GetMonitors();
        if (monitors.Count > 0 && !monitors.Any(m => m.IsPrimary))
            monitors[0] = monitors[0] with { IsPrimary = true };

        foreach (var m in monitors)
        {
            var w = new LockWindow(this, m);
            _windows.Add(w);
            w.Show();
        }

        IntPtr module = GetModuleHandle(null);
        _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, module, 0);
        _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, module, 0);

        SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
        SystemEvents.SessionSwitch += OnSessionSwitch;

        BringPrimaryToFront();
        _timer.Start();
    }

    private void End(string reason)
    {
        if (!_active) return;
        _active = false;
        _timer.Stop();
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        if (_keyboardHook != IntPtr.Zero) UnhookWindowsHookEx(_keyboardHook);
        if (_mouseHook != IntPtr.Zero) UnhookWindowsHookEx(_mouseHook);
        SetThreadExecutionState(ES_CONTINUOUS);
        foreach (var w in _windows) w.ForceClose();
        _windows.Clear();
        _onEnded(reason);
    }

    public async Task TryPassword(string password)
    {
        // Hash on a background thread: if the UI thread blocks, the hooks lag.
        bool ok = await Task.Run(() => PasswordStore.Verify(password));
        if (!_active) return;
        if (ok)
        {
            End(_wrongAttempts > 0 ? Strings.UnlockedWithAttempts(_wrongAttempts) : Strings.Unlocked);
            return;
        }
        _wrongAttempts++;
        Primary?.ShowMessage(Strings.WrongPassword(_wrongAttempts));
        await Task.Delay(Math.Min(_wrongAttempts, 5) * 400); // slow down repeated guesses
    }

    // ---------------- Periodic check ----------------

    private void Check()
    {
        if (!_active) return;

        var elapsed = DateTime.Now - _startedAt;
        if (_testDuration is { } test)
        {
            var left = test - elapsed;
            if (left <= TimeSpan.Zero) { End(Strings.TestExpired); return; }
            Primary?.ShowStatus(Strings.TestCountdown((int)Math.Ceiling(left.TotalSeconds)));
        }
        else
        {
            Primary?.ShowStatus(Strings.LockedFor(elapsed.ToString(@"hh\:mm\:ss")));
        }

        if (!IsOnDefaultDesktop())
        {
            // Ctrl+Alt+Del screen, UAC prompt or Windows lock screen: hand over to the real Windows lock.
            if (!_windowsLockRequested) RequestWindowsLock();
            return;
        }

        if (_windowsLockRequested)
        {
            if (_sessionWasLocked)
            {
                // Back from the Windows lock: the Windows password was entered.
                End(Strings.UnlockedViaWindows);
                return;
            }
            // The lock didn't take (e.g. Task Manager was picked from Ctrl+Alt+Del): retry a few times.
            if (_windowsLockAttempts < 4 && DateTime.Now - _lastWindowsLockAttempt > TimeSpan.FromSeconds(1))
                RequestWindowsLock();
        }

        // Keep the lock windows on top and focused.
        foreach (var w in _windows) w.PlaceOnMonitor();
        if (!IsOurWindow(GetForegroundWindow())) BringPrimaryToFront();
    }

    private void RequestWindowsLock()
    {
        _windowsLockRequested = true;
        _windowsLockAttempts++;
        _lastWindowsLockAttempt = DateTime.Now;
        LockWorkStation();
    }

    private void OnSessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        // SystemEvents may call us on its own thread; switch to the UI thread.
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            if (!_active) return;
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                _windowsLockRequested = true;
                _sessionWasLocked = true;
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock && _sessionWasLocked)
            {
                End(Strings.UnlockedViaWindows);
            }
        });
    }

    public void BringPrimaryToFront()
    {
        var primary = Primary;
        if (primary is null || primary.Handle == IntPtr.Zero) return;
        IntPtr fg = GetForegroundWindow();
        uint fgThread = GetWindowThreadProcessId(fg, out _);
        uint us = GetCurrentThreadId();
        bool attached = fgThread != 0 && fgThread != us && AttachThreadInput(us, fgThread, true);
        SetForegroundWindow(primary.Handle);
        primary.Activate();
        if (attached) AttachThreadInput(us, fgThread, false);
        primary.FocusPassword();
    }

    private bool IsOurWindow(IntPtr h)
    {
        if (h == IntPtr.Zero) return false;
        IntPtr root = GetAncestor(h, GA_ROOT);
        foreach (var w in _windows)
            if (w.Handle == h || w.Handle == root) return true;
        return false;
    }

    // ---------------- Hooks ----------------

    private const int VK_TAB = 0x09, VK_ESCAPE = 0x1B, VK_SPACE = 0x20, VK_F4 = 0x73,
        VK_LWIN = 0x5B, VK_RWIN = 0x5C, VK_APPS = 0x5D, VK_CONTROL = 0x11;

    private IntPtr KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _active)
        {
            var k = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            if (ShouldBlockKey(k)) return 1;
        }
        return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
    }

    private bool ShouldBlockKey(KBDLLHOOKSTRUCT k)
    {
        // If focus isn't ours, no key may reach any other window (e.g. a terminal).
        if (!IsOurWindow(GetForegroundWindow())) return true;

        int vk = (int)k.vkCode;
        if (vk is VK_LWIN or VK_RWIN or VK_APPS) return true;                     // Start menu, Win shortcuts
        bool alt = (k.flags & LLKHF_ALTDOWN) != 0;
        if (alt && vk is VK_TAB or VK_ESCAPE or VK_F4 or VK_SPACE) return true;  // Alt+Tab, Alt+F4, Alt+Esc
        bool ctrl = GetAsyncKeyState(VK_CONTROL) < 0;
        if (ctrl && vk == VK_ESCAPE) return true;                                 // Ctrl+Esc, Ctrl+Shift+Esc
        return false; // everything else (including AltGr characters like @ and €) goes to the password box
    }

    private const int WM_MOUSEMOVE = 0x0200;

    private IntPtr MouseHook(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _active && (int)wParam != WM_MOUSEMOVE)
        {
            // Clicks and the wheel may only reach our lock windows.
            var m = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            if (!IsOurWindow(WindowFromPoint(m.pt))) return 1;
        }
        return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }
}
