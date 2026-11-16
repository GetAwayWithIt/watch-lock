using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using static WatchLock.NativeMethods;

namespace WatchLock;

public partial class LockWindow : Window
{
    private readonly LockSession _session;
    private readonly RECT _bounds;
    private bool _canClose;

    public bool IsPrimary { get; }
    public IntPtr Handle { get; private set; }

    internal LockWindow(LockSession session, NativeMethods.Monitor monitor)
    {
        InitializeComponent();
        _session = session;
        _bounds = monitor.Bounds;
        IsPrimary = monitor.IsPrimary;
        if (IsPrimary) Card.Visibility = Visibility.Visible;
        else Badge.Text = Strings.LockedBadgeSecondary;

        SourceInitialized += (_, _) =>
        {
            Handle = new WindowInteropHelper(this).Handle;
            PlaceOnMonitor();
        };
        Activated += (_, _) => { if (!IsPrimary) _session.BringPrimaryToFront(); else Password.Focus(); };
    }

    /// <summary>Covers the whole monitor (taskbar included), in physical pixels.</summary>
    public void PlaceOnMonitor() =>
        SetWindowPos(Handle, HWND_TOPMOST, _bounds.Left, _bounds.Top,
            _bounds.Right - _bounds.Left, _bounds.Bottom - _bounds.Top, SWP_SHOWWINDOW);

    public void ShowStatus(string text) => Status.Text = text;

    public void ShowMessage(string text) => Message.Text = text;

    public void FocusPassword() => Password.Focus();

    private async void Password_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        string entered = Password.Password;
        Password.Clear();
        if (entered.Length == 0) return;
        Password.IsEnabled = false;
        await _session.TryPassword(entered);
        if (IsLoaded) { Password.IsEnabled = true; Password.Focus(); }
    }

    public void ForceClose()
    {
        _canClose = true;
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_canClose) e.Cancel = true; // Alt+F4 and friends can't close it
        base.OnClosing(e);
    }
}
