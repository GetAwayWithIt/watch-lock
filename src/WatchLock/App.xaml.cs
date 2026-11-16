using System.Windows;
using System.Windows.Threading;
using static WatchLock.NativeMethods;

namespace WatchLock;

/// <summary>
/// The app lives in the system tray. The main window is only created when needed
/// and fully released from memory when closed.
/// </summary>
public partial class App : Application
{
    private const string SingleInstanceName = @"Local\WatchLock_SingleInstance";
    private const string ShowSignalName = @"Local\WatchLock_Show";

    private Mutex? _singleInstance;
    private EventWaitHandle? _showSignal;
    private RegisteredWaitHandle? _showWaiter;

    private TrayIcon? _tray;
    private MainWindow? _window;
    private LockSession? _session;
    private DispatcherTimer? _trimTimer;
    private bool _reopening;

    public Settings Settings { get; private set; } = new();
    public static new App Current => (App)Application.Current;
    public bool HotkeyActive => _tray?.HotkeyActive ?? false;

    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstance = new Mutex(true, SingleInstanceName, out bool isFirst);
        if (!isFirst)
        {
            // Already running: ask the running copy to show its window, then quit.
            try { EventWaitHandle.OpenExisting(ShowSignalName).Set(); } catch { }
            Shutdown();
            return;
        }

        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        Settings = Settings.Load();
        if (PasswordStore.MigrateFromOldVersion())
        {
            // Users of v0.2 ("Ekran Kilidi") had a Turkish-only interface: keep it Turkish for them.
            Settings.Language = "tr";
            Settings.Save();
        }
        Strings.SetLanguage(Settings.Language);

        _showSignal = new EventWaitHandle(false, EventResetMode.AutoReset, ShowSignalName);
        _showWaiter = ThreadPool.RegisterWaitForSingleObject(_showSignal,
            (_, _) => Dispatcher.BeginInvoke(ShowWindow), null, Timeout.Infinite, false);

        _tray = new TrayIcon();
        _tray.OpenRequested += ShowWindow;
        _tray.LockRequested += () => Lock(fromWindow: false);
        _tray.ExitRequested += ExitApp;

        ShowWindow();
    }

    public void ShowWindow()
    {
        if (_session is not null) return;
        _trimTimer?.Stop();
        if (_window is null)
        {
            _window = new MainWindow();
            _window.Closed += (_, _) =>
            {
                _window = null;
                if (_reopening || _session is not null) return;
                if (!Settings.TrayTipShown)
                {
                    _tray?.Notify(Strings.BalloonBgTitle, Strings.BalloonBgText);
                    Settings.TrayTipShown = true;
                    Settings.Save();
                }
                ScheduleTrim();
            };
            _window.Show();
        }
        if (_window.WindowState == WindowState.Minimized) _window.WindowState = WindowState.Normal;
        _window.Activate();
    }

    public void ChangeLanguage(string code)
    {
        Settings.Language = code;
        Settings.Save();
        Strings.SetLanguage(code);
        _tray?.RefreshTooltip();

        // Window texts are read when the window is created: reopen it at the same spot.
        if (_window is { } old)
        {
            double left = old.Left, top = old.Top;
            _reopening = true;
            old.Close();
            _reopening = false;
            ShowWindow();
            if (_window is not null)
            {
                _window.WindowStartupLocation = WindowStartupLocation.Manual;
                _window.Left = left;
                _window.Top = top;
            }
        }
    }

    public void Lock(bool fromWindow)
    {
        if (_session is not null) return;
        if (!PasswordStore.HasPassword)
        {
            ShowWindow(); // a password has to be set first
            return;
        }

        _trimTimer?.Stop();
        TimeSpan? test = Settings.TestMode ? TimeSpan.FromSeconds(60) : null;

        _session = new LockSession(test, reason =>
        {
            _session = null;
            if (_tray is not null) _tray.Locked = false;
            if (fromWindow)
            {
                ShowWindow();
                _window?.ShowInfo(reason);
            }
            else
            {
                _tray?.Notify(Strings.AppName, reason);
                ScheduleTrim();
            }
        });
        if (_tray is not null) _tray.Locked = true;
        _window?.Close(); // no need for the main window while locked (closed after _session is set)
        _session.Start();
    }

    /// <summary>Gives memory back a moment after the window closes, once WPF has released its resources.</summary>
    private void ScheduleTrim()
    {
        if (_trimTimer is null)
        {
            _trimTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = TimeSpan.FromSeconds(2) };
            _trimTimer.Tick += TrimTick;
        }
        _trimTimer.Stop();
        _trimTimer.Start();
    }

    private void TrimTick(object? sender, EventArgs e)
    {
        _trimTimer!.Stop();
        if (_window is not null || _session is not null) return;
        GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        SetProcessWorkingSetSize(GetCurrentProcess(), new IntPtr(-1), new IntPtr(-1));
    }

    private void ExitApp()
    {
        if (_session is not null) return;
        _window?.Close();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _showWaiter?.Unregister(null);
        _showSignal?.Dispose();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}
