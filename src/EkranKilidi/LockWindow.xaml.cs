using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using static EkranKilidi.NativeMethods;

namespace EkranKilidi;

public partial class LockWindow : Window
{
    private readonly LockSession _oturum;
    private readonly RECT _alan;
    public bool Birincil { get; }
    public IntPtr Hwnd { get; private set; }
    private bool _kapanabilir;

    internal LockWindow(LockSession oturum, NativeMethods.Monitor monitor)
    {
        InitializeComponent();
        _oturum = oturum;
        _alan = monitor.Alan;
        Birincil = monitor.Birincil;
        if (Birincil) Kart.Visibility = Visibility.Visible;
        else Serit.Text = "🔒 Kilitli: şifre ana ekranda";

        SourceInitialized += (_, _) =>
        {
            Hwnd = new WindowInteropHelper(this).Handle;
            Yerlestir();
        };
        Activated += (_, _) => { if (!Birincil) _oturum.AnaPencereyiOneAl(); else Sifre.Focus(); };
    }

    /// <summary>Pencereyi monitörün tamamına (görev çubuğu dahil) fiziksel piksellerle yerleştirir.</summary>
    public void Yerlestir() =>
        SetWindowPos(Hwnd, HWND_TOPMOST, _alan.Left, _alan.Top,
            _alan.Right - _alan.Left, _alan.Bottom - _alan.Top, SWP_SHOWWINDOW);

    public void SureyiGoster(string metin) => Sure.Text = metin;

    public void MesajGoster(string metin) => Mesaj.Text = metin;

    public void SifreyeOdaklan() => Sifre.Focus();

    private async void Sifre_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        string girilen = Sifre.Password;
        Sifre.Clear();
        if (girilen.Length == 0) return;
        Sifre.IsEnabled = false;
        await _oturum.SifreDene(girilen);
        if (IsLoaded) { Sifre.IsEnabled = true; Sifre.Focus(); }
    }

    public void Kapat()
    {
        _kapanabilir = true;
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_kapanabilir) e.Cancel = true; // Alt+F4 vb. ile kapanmasın
        base.OnClosing(e);
    }
}
