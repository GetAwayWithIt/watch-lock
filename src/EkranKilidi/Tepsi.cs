using System.Runtime.InteropServices;
using System.Windows.Interop;
using static EkranKilidi.NativeMethods;

namespace EkranKilidi;

/// <summary>
/// Sistem tepsisi simgesi + genel kısayol tuşu. WinForms'u yüklememek (RAM) için doğrudan Win32 ile yapılıyor.
/// </summary>
public sealed class Tepsi : IDisposable
{
    public const string KisayolMetni = "Ctrl+AltGr+F";

    private const uint WM_TEPSI = 0x8000 + 1; // WM_APP + 1
    private const int WM_LBUTTONUP = 0x0202, WM_RBUTTONUP = 0x0205;
    private const int MENU_AC = 1, MENU_KILITLE = 2, MENU_CIKIS = 3;

    private readonly HwndSource _pencere;
    private readonly uint _gorevCubuguYenilendi;
    private IntPtr _ikon;
    private bool _kilitli;

    // Ctrl+AltGr+F: Windows AltGr'yi "sahte sol Ctrl + sağ Alt" olarak gönderir, bu yüzden
    // RegisterHotKey AltGr+F ile Ctrl+AltGr+F'yi ayıramaz. Tuşları alt seviye hook ile kendimiz izliyoruz.
    private readonly HookProc _klavyeProc;
    private IntPtr _klavyeHook;
    private bool _solCtrl, _sagCtrl, _altGr, _fYutuldu;
    private bool _solCtrlOnceki;
    private uint _solCtrlZamani;

    public event Action? PencereIstendi;
    public event Action? KilitIstendi;
    public event Action? CikisIstendi;

    public bool KisayolKayitli { get; }

    public Tepsi()
    {
        // Görünmez üst seviye pencere: tepsi mesajlarını ve kısayolu alır.
        _pencere = new HwndSource(new HwndSourceParameters("EkranKilidiTepsi") { Width = 0, Height = 0, WindowStyle = 0 });
        _pencere.AddHook(MesajIsle);
        _gorevCubuguYenilendi = RegisterWindowMessage("TaskbarCreated");

        var kucuk = new IntPtr[1];
        if (Environment.ProcessPath is { } yol && ExtractIconEx(yol, 0, null, kucuk, 1) > 0 && kucuk[0] != IntPtr.Zero)
            _ikon = kucuk[0];
        else
            _ikon = LoadIcon(IntPtr.Zero, new IntPtr(32512));

        Ekle();
        _klavyeProc = KlavyeHook;
        _klavyeHook = SetWindowsHookEx(WH_KEYBOARD_LL, _klavyeProc, GetModuleHandle(null), 0);
        KisayolKayitli = _klavyeHook != IntPtr.Zero;
    }

    private const int VK_F = 0x46, VK_LCONTROL = 0xA2, VK_RCONTROL = 0xA3, VK_RMENU = 0xA5;
    private const uint LLKHF_UP = 0x80;

    private IntPtr KlavyeHook(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var k = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            bool basili = (k.flags & LLKHF_UP) == 0;
            switch ((int)k.vkCode)
            {
                case VK_LCONTROL:
                    // AltGr'nin ürettiği sahte Ctrl'nin tarama kodunda 0x200 biti olur; onu sayma.
                    if ((k.scanCode & 0x200) == 0)
                    {
                        if (basili) { _solCtrlOnceki = _solCtrl; _solCtrlZamani = k.time; }
                        _solCtrl = basili;
                    }
                    break;
                case VK_RCONTROL:
                    _sagCtrl = basili;
                    break;
                case VK_RMENU:
                    // Yedek kontrol: sahte Ctrl, AltGr ile aynı anda (aynı zaman damgasıyla) gelir.
                    if (basili && k.time == _solCtrlZamani) _solCtrl = _solCtrlOnceki;
                    _altGr = basili;
                    break;
                case VK_F:
                    if (basili && !_kilitli && KisayolBasili())
                    {
                        _fYutuldu = true;
                        _pencere.Dispatcher.BeginInvoke(() => KilitIstendi?.Invoke());
                        return 1; // "F" harfi o an açık olan programa gitmesin
                    }
                    if (!basili && _fYutuldu) { _fYutuldu = false; return 1; }
                    break;
            }
        }
        return CallNextHookEx(_klavyeHook, nCode, wParam, lParam);
    }

    private bool KisayolBasili()
    {
        // Kendi tuttuğumuz durumu Windows'un gerçek tuş durumuyla da doğrula (kaçan olay olursa diye).
        bool altGr = _altGr && GetAsyncKeyState(VK_RMENU) < 0;
        bool ctrl = (_sagCtrl && GetAsyncKeyState(VK_RCONTROL) < 0)
                 || (_solCtrl && GetAsyncKeyState(VK_LCONTROL) < 0);
        return altGr && ctrl;
    }

    public bool Kilitli
    {
        get => _kilitli;
        set { _kilitli = value; IpucuGuncelle(); }
    }

    private NOTIFYICONDATA Veri(uint bayraklar) => new()
    {
        cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
        hWnd = _pencere.Handle,
        uID = 1,
        uFlags = bayraklar,
        uCallbackMessage = WM_TEPSI,
        hIcon = _ikon,
        szTip = _kilitli ? "Ekran Kilidi: kilitli" : $"Ekran Kilidi: kilitlemek için {KisayolMetni}",
        szInfo = "",
        szInfoTitle = "",
    };

    private void Ekle()
    {
        var v = Veri(NIF_MESSAGE | NIF_ICON | NIF_TIP);
        Shell_NotifyIcon(NIM_ADD, ref v);
    }

    private void IpucuGuncelle()
    {
        var v = Veri(NIF_TIP);
        Shell_NotifyIcon(NIM_MODIFY, ref v);
    }

    /// <summary>Tepsiden küçük bir bildirim balonu gösterir.</summary>
    public void Bildir(string baslik, string metin)
    {
        var v = Veri(NIF_INFO);
        v.szInfoTitle = baslik;
        v.szInfo = metin;
        v.dwInfoFlags = NIIF_USER;
        Shell_NotifyIcon(NIM_MODIFY, ref v);
    }

    private IntPtr MesajIsle(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_TEPSI)
        {
            handled = true;
            if (_kilitli) return IntPtr.Zero; // kilitliyken tepsi menüsü çalışmasın
            int fare = lParam.ToInt32() & 0xFFFF;
            if (fare == WM_LBUTTONUP) PencereIstendi?.Invoke();
            else if (fare == WM_RBUTTONUP) MenuGoster();
        }
        else if (msg == (int)_gorevCubuguYenilendi)
        {
            Ekle(); // Explorer yeniden başlarsa simgeyi geri koy
        }
        return IntPtr.Zero;
    }

    private void MenuGoster()
    {
        IntPtr menu = CreatePopupMenu();
        AppendMenu(menu, MF_STRING, (UIntPtr)MENU_AC, "Ekran Kilidi'ni aç");
        AppendMenu(menu, MF_STRING, (UIntPtr)MENU_KILITLE, $"Kilitle\t{KisayolMetni}");
        AppendMenu(menu, MF_SEPARATOR, UIntPtr.Zero, null);
        AppendMenu(menu, MF_STRING, (UIntPtr)MENU_CIKIS, "Çıkış");

        GetCursorPos(out var p);
        SetForegroundWindow(_pencere.Handle); // menü dışına tıklayınca kapansın diye gerekli
        int secim = TrackPopupMenu(menu, TPM_RETURNCMD | TPM_RIGHTBUTTON | TPM_BOTTOMALIGN, p.X, p.Y, 0, _pencere.Handle, IntPtr.Zero);
        PostMessage(_pencere.Handle, 0, IntPtr.Zero, IntPtr.Zero);
        DestroyMenu(menu);

        switch (secim)
        {
            case MENU_AC: PencereIstendi?.Invoke(); break;
            case MENU_KILITLE: KilitIstendi?.Invoke(); break;
            case MENU_CIKIS: CikisIstendi?.Invoke(); break;
        }
    }

    public void Dispose()
    {
        if (_klavyeHook != IntPtr.Zero) UnhookWindowsHookEx(_klavyeHook);
        _klavyeHook = IntPtr.Zero;
        var v = Veri(0);
        Shell_NotifyIcon(NIM_DELETE, ref v);
        if (_ikon != IntPtr.Zero) DestroyIcon(_ikon);
        _ikon = IntPtr.Zero;
        _pencere.Dispose();
    }
}
