using System.Windows;
using System.Windows.Threading;
using static EkranKilidi.NativeMethods;

namespace EkranKilidi;

/// <summary>
/// Uygulama tepside yaşar. Ana pencere sadece gerektiğinde açılır, kapanınca bellekten tamamen atılır.
/// </summary>
public partial class App : Application
{
    private const string AdKopya = @"Local\EkranKilidi_TekKopya";
    private const string AdGoster = @"Local\EkranKilidi_Goster";

    private Mutex? _tekKopya;
    private EventWaitHandle? _gosterSinyali;
    private RegisteredWaitHandle? _gosterBekleyici;

    private Tepsi? _tepsi;
    private MainWindow? _pencere;
    private LockSession? _oturum;
    private DispatcherTimer? _bellekZamanlayici;

    public Tercihler Tercihler { get; private set; } = new();
    public static new App Current => (App)Application.Current;

    protected override void OnStartup(StartupEventArgs e)
    {
        _tekKopya = new Mutex(true, AdKopya, out bool yeni);
        if (!yeni)
        {
            // Zaten açık: açık olan kopyaya "pencereni göster" de ve çık.
            try { EventWaitHandle.OpenExisting(AdGoster).Set(); } catch { }
            Shutdown();
            return;
        }

        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        Tercihler = Tercihler.Yukle();

        _gosterSinyali = new EventWaitHandle(false, EventResetMode.AutoReset, AdGoster);
        _gosterBekleyici = ThreadPool.RegisterWaitForSingleObject(_gosterSinyali,
            (_, _) => Dispatcher.BeginInvoke(PencereyiGoster), null, Timeout.Infinite, false);

        _tepsi = new Tepsi();
        _tepsi.PencereIstendi += PencereyiGoster;
        _tepsi.KilitIstendi += () => Kilitle(pencereden: false);
        _tepsi.CikisIstendi += Cik;

        PencereyiGoster();
    }

    public bool KisayolCalisiyor => _tepsi?.KisayolKayitli ?? false;

    public void PencereyiGoster()
    {
        if (_oturum is not null) return;
        _bellekZamanlayici?.Stop();
        if (_pencere is null)
        {
            _pencere = new MainWindow();
            _pencere.Closed += (_, _) =>
            {
                _pencere = null;
                if (!Tercihler.TepsiIpucuGosterildi && _oturum is null)
                {
                    _tepsi?.Bildir("Ekran Kilidi arka planda", $"Kilitlemek için {Tepsi.KisayolMetni}. Kapatmak için tepsideki simgeye sağ tıkla.");
                    Tercihler.TepsiIpucuGosterildi = true;
                    Tercihler.Kaydet();
                }
                BellegiAzaltPlanla();
            };
            _pencere.Show();
        }
        if (_pencere.WindowState == WindowState.Minimized) _pencere.WindowState = WindowState.Normal;
        _pencere.Activate();
    }

    public void Kilitle(bool pencereden)
    {
        if (_oturum is not null) return;
        if (!PasswordStore.SifreVar)
        {
            PencereyiGoster(); // önce şifre belirlenmeli
            return;
        }

        _bellekZamanlayici?.Stop();
        TimeSpan? deneme = Tercihler.DenemeModu ? TimeSpan.FromSeconds(60) : null;

        _oturum = new LockSession(deneme, neden =>
        {
            _oturum = null;
            if (_tepsi is not null) _tepsi.Kilitli = false;
            if (pencereden)
            {
                PencereyiGoster();
                _pencere?.Bilgi(neden);
            }
            else
            {
                _tepsi?.Bildir("Ekran Kilidi", neden);
                BellegiAzaltPlanla();
            }
        });
        if (_tepsi is not null) _tepsi.Kilitli = true;
        _pencere?.Close(); // kilit sırasında ana pencereye gerek yok (_oturum atandıktan sonra kapat)
        _oturum.Baslat();
    }

    /// <summary>Pencere kapandıktan biraz sonra, WPF kaynaklarını bıraktığında belleği geri verir.</summary>
    private void BellegiAzaltPlanla()
    {
        _bellekZamanlayici ??= new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = TimeSpan.FromSeconds(2) };
        _bellekZamanlayici.Stop();
        _bellekZamanlayici.Tick -= BellekTick;
        _bellekZamanlayici.Tick += BellekTick;
        _bellekZamanlayici.Start();
    }

    private void BellekTick(object? s, EventArgs e)
    {
        _bellekZamanlayici!.Stop();
        if (_pencere is not null || _oturum is not null) return;
        GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        SetProcessWorkingSetSize(GetCurrentProcess(), new IntPtr(-1), new IntPtr(-1));
    }

    private void Cik()
    {
        if (_oturum is not null) return;
        _pencere?.Close();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tepsi?.Dispose();
        _gosterBekleyici?.Unregister(null);
        _gosterSinyali?.Dispose();
        _tekKopya?.Dispose();
        base.OnExit(e);
    }
}
