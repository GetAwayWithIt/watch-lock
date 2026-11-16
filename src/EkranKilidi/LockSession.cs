using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.Win32;
using static EkranKilidi.NativeMethods;

namespace EkranKilidi;

/// <summary>
/// Bir kilit oturumu: her monitöre yarı saydam pencere açar, klavye/fare hook'larını kurar,
/// uyku engeller ve Ctrl+Alt+Del gibi kaçışlarda Windows kilidine düşer.
/// </summary>
public sealed class LockSession
{
    private readonly List<LockWindow> _pencereler = new();
    private readonly DispatcherTimer _zamanlayici;
    private readonly DateTime _baslangic = DateTime.Now;
    private readonly TimeSpan? _denemeSuresi;
    private readonly Action<string> _bitti;

    // Hook delegeleri GC tarafından toplanmasın diye alanda tutuluyor.
    private readonly HookProc _klavyeProc, _fareProc;
    private IntPtr _klavyeHook, _fareHook;

    private bool _aktif;
    private int _yanlisSayisi;
    private bool _windowsKilidiIstendi, _oturumKilitlendi;
    private int _kilitDenemesi;
    private DateTime _sonKilitDenemesi;

    public LockSession(TimeSpan? denemeSuresi, Action<string> bitti)
    {
        _denemeSuresi = denemeSuresi;
        _bitti = bitti;
        _klavyeProc = KlavyeHook;
        _fareProc = FareHook;
        _zamanlayici = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromMilliseconds(250) };
        _zamanlayici.Tick += (_, _) => Kontrol();
    }

    private LockWindow? Ana => _pencereler.FirstOrDefault(p => p.Birincil) ?? _pencereler.FirstOrDefault();

    public void Baslat()
    {
        _aktif = true;
        var monitorler = Monitorler();
        if (monitorler.Count > 0 && !monitorler.Any(m => m.Birincil))
            monitorler[0] = monitorler[0] with { Birincil = true };

        foreach (var m in monitorler)
        {
            var p = new LockWindow(this, m);
            _pencereler.Add(p);
            p.Show();
        }

        IntPtr mod = GetModuleHandle(null);
        _klavyeHook = SetWindowsHookEx(WH_KEYBOARD_LL, _klavyeProc, mod, 0);
        _fareHook = SetWindowsHookEx(WH_MOUSE_LL, _fareProc, mod, 0);

        SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
        SystemEvents.SessionSwitch += OturumDegisti;

        AnaPencereyiOneAl();
        _zamanlayici.Start();
    }

    private void Bitir(string neden)
    {
        if (!_aktif) return;
        _aktif = false;
        _zamanlayici.Stop();
        SystemEvents.SessionSwitch -= OturumDegisti;
        if (_klavyeHook != IntPtr.Zero) UnhookWindowsHookEx(_klavyeHook);
        if (_fareHook != IntPtr.Zero) UnhookWindowsHookEx(_fareHook);
        SetThreadExecutionState(ES_CONTINUOUS);
        foreach (var p in _pencereler) p.Kapat();
        _pencereler.Clear();
        _bitti(neden);
    }

    public async Task SifreDene(string sifre)
    {
        // Hash'i arka planda hesapla: UI iş parçacığı bloklanırsa hook'lar gecikir.
        bool dogru = await Task.Run(() => PasswordStore.Dogrula(sifre));
        if (!_aktif) return;
        if (dogru)
        {
            Bitir(_yanlisSayisi > 0
                ? $"Kilit açıldı. Bu sırada {_yanlisSayisi} yanlış şifre denendi."
                : "Kilit açıldı.");
            return;
        }
        _yanlisSayisi++;
        Ana?.MesajGoster($"Yanlış şifre ({_yanlisSayisi})");
        await Task.Delay(Math.Min(_yanlisSayisi, 5) * 400); // art arda denemeyi yavaşlat
    }

    // ---------------- Periyodik kontrol ----------------

    private void Kontrol()
    {
        if (!_aktif) return;

        var gecen = DateTime.Now - _baslangic;
        if (_denemeSuresi is { } ds)
        {
            var kalan = ds - gecen;
            if (kalan <= TimeSpan.Zero) { Bitir("Deneme modu: süre doldu, kilit kendiliğinden açıldı."); return; }
            Ana?.SureyiGoster($"Deneme modu: {Math.Ceiling(kalan.TotalSeconds)} sn sonra kendiliğinden açılacak");
        }
        else
        {
            Ana?.SureyiGoster(gecen.ToString(@"hh\:mm\:ss") + " süredir kilitli");
        }

        bool normal = NormalMasaustundeMi();

        if (!normal)
        {
            // Ctrl+Alt+Del ekranı, UAC ya da Windows kilit ekranı: gerçek Windows kilidine düş.
            if (!_windowsKilidiIstendi) WindowsKilidi();
            return;
        }

        if (_windowsKilidiIstendi)
        {
            if (_oturumKilitlendi)
            {
                // Windows kilidinden geri dönüldü: Windows şifresi girildi demektir.
                Bitir("Windows şifresiyle açıldı.");
                return;
            }
            // Kilit tutmadı (ör. Ctrl+Alt+Del'den Görev Yöneticisi seçildi): birkaç kez daha dene.
            if (_kilitDenemesi < 4 && DateTime.Now - _sonKilitDenemesi > TimeSpan.FromSeconds(1))
                WindowsKilidi();
        }

        // Kilit pencereleri her zaman en üstte ve odakta kalsın.
        foreach (var p in _pencereler) p.Yerlestir();
        if (!BizimPencereMi(GetForegroundWindow())) AnaPencereyiOneAl();
    }

    private void WindowsKilidi()
    {
        _windowsKilidiIstendi = true;
        _kilitDenemesi++;
        _sonKilitDenemesi = DateTime.Now;
        LockWorkStation();
    }

    private void OturumDegisti(object? s, SessionSwitchEventArgs e)
    {
        // SystemEvents kendi iş parçacığında çağırabilir; UI iş parçacığına geç.
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            if (!_aktif) return;
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                _windowsKilidiIstendi = true;
                _oturumKilitlendi = true;
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock && _oturumKilitlendi)
            {
                Bitir("Windows şifresiyle açıldı.");
            }
        });
    }

    public void AnaPencereyiOneAl()
    {
        var ana = Ana;
        if (ana is null || ana.Hwnd == IntPtr.Zero) return;
        IntPtr fg = GetForegroundWindow();
        uint fgThread = GetWindowThreadProcessId(fg, out _);
        uint biz = GetCurrentThreadId();
        bool bagli = fgThread != 0 && fgThread != biz && AttachThreadInput(biz, fgThread, true);
        SetForegroundWindow(ana.Hwnd);
        ana.Activate();
        if (bagli) AttachThreadInput(biz, fgThread, false);
        ana.SifreyeOdaklan();
    }

    private bool BizimPencereMi(IntPtr h)
    {
        if (h == IntPtr.Zero) return false;
        IntPtr kok = GetAncestor(h, GA_ROOT);
        foreach (var p in _pencereler)
            if (p.Hwnd == h || p.Hwnd == kok) return true;
        return false;
    }

    // ---------------- Hook'lar ----------------

    private const int VK_TAB = 0x09, VK_ESCAPE = 0x1B, VK_SPACE = 0x20, VK_F4 = 0x73,
        VK_LWIN = 0x5B, VK_RWIN = 0x5C, VK_APPS = 0x5D, VK_CONTROL = 0x11;

    private IntPtr KlavyeHook(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _aktif)
        {
            var k = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            if (EngelleKlavye(k)) return 1;
        }
        return CallNextHookEx(_klavyeHook, nCode, wParam, lParam);
    }

    private bool EngelleKlavye(KBDLLHOOKSTRUCT k)
    {
        // Odak bizde değilse hiçbir tuş başka pencereye (ör. terminale) gitmesin.
        if (!BizimPencereMi(GetForegroundWindow())) return true;

        int vk = (int)k.vkCode;
        if (vk is VK_LWIN or VK_RWIN or VK_APPS) return true;                     // Başlat menüsü, Win kısayolları
        bool alt = (k.flags & LLKHF_ALTDOWN) != 0;
        if (alt && vk is VK_TAB or VK_ESCAPE or VK_F4 or VK_SPACE) return true;  // Alt+Tab, Alt+F4, Alt+Esc
        bool ctrl = GetAsyncKeyState(VK_CONTROL) < 0;
        if (ctrl && vk == VK_ESCAPE) return true;                                 // Ctrl+Esc, Ctrl+Shift+Esc
        return false; // AltGr (@, € vb.) dahil diğer tuşlar şifre kutusuna gider
    }

    private const int WM_MOUSEMOVE = 0x0200;

    private IntPtr FareHook(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _aktif && (int)wParam != WM_MOUSEMOVE)
        {
            // Tıklama ve tekerlek sadece kilit pencerelerimize gidebilir.
            var m = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            if (!BizimPencereMi(WindowFromPoint(m.pt))) return 1;
        }
        return CallNextHookEx(_fareHook, nCode, wParam, lParam);
    }
}
