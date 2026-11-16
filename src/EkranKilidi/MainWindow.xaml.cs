using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EkranKilidi;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DenemeModu.IsChecked = App.Current.Tercihler.DenemeModu;
        KisayolBilgisi.Text = App.Current.KisayolCalisiyor
            ? $"Kısayol: {Tepsi.KisayolMetni}"
            : "Kısayol tuşu başlatılamadı, Kilitle butonunu kullan.";
        if (PasswordStore.SifreVar) KilitEkraniniGoster();
        else KurulumuGoster(degistir: false);
    }

    private void KurulumuGoster(bool degistir)
    {
        KurulumPaneli.Visibility = Visibility.Visible;
        KilitPaneli.Visibility = Visibility.Collapsed;
        EskiSifrePaneli.Visibility = degistir ? Visibility.Visible : Visibility.Collapsed;
        VazgecButon.Visibility = degistir ? Visibility.Visible : Visibility.Collapsed;
        KurulumBaslik.Text = degistir ? "Şifreyi değiştir" : "Bir kilit şifresi belirle";
        EskiSifre.Clear(); YeniSifre1.Clear(); YeniSifre2.Clear();
        Durum.Text = "";
        (degistir ? EskiSifre : YeniSifre1).Focus();
    }

    private void KilitEkraniniGoster()
    {
        KurulumPaneli.Visibility = Visibility.Collapsed;
        KilitPaneli.Visibility = Visibility.Visible;
    }

    private async void Kaydet_Click(object sender, RoutedEventArgs e)
    {
        Hata("");
        bool degistir = EskiSifrePaneli.Visibility == Visibility.Visible;
        string eski = EskiSifre.Password, s1 = YeniSifre1.Password, s2 = YeniSifre2.Password;

        if (s1.Length < 4) { Hata("Şifre en az 4 karakter olsun."); return; }
        if (s1 != s2) { Hata("İki şifre aynı değil."); return; }

        KaydetButon.IsEnabled = false;
        try
        {
            if (degistir && !await Task.Run(() => PasswordStore.Dogrula(eski)))
            {
                Hata("Mevcut şifre yanlış.");
                return;
            }
            await Task.Run(() => PasswordStore.Kaydet(s1));
            KilitEkraniniGoster();
            Bilgi(degistir ? "Şifre değişti." : "Şifre kaydedildi. Hazırsın.");
        }
        finally { KaydetButon.IsEnabled = true; }
    }

    private void YeniSifre2_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) Kaydet_Click(sender, e);
    }

    private void Vazgec_Click(object sender, RoutedEventArgs e)
    {
        KilitEkraniniGoster();
        Durum.Text = "";
    }

    private void SifreDegistir_Click(object sender, RoutedEventArgs e) => KurulumuGoster(degistir: true);

    private void Kilitle_Click(object sender, RoutedEventArgs e) => App.Current.Kilitle(pencereden: true);

    private void DenemeModu_Degisti(object sender, RoutedEventArgs e)
    {
        var t = App.Current.Tercihler;
        bool yeni = DenemeModu.IsChecked == true;
        if (t.DenemeModu == yeni) return;
        t.DenemeModu = yeni;
        t.Kaydet();
    }

    private void Hata(string m) { Durum.Foreground = (Brush)FindResource("Danger"); Durum.Text = m; }
    public void Bilgi(string m) { Durum.Foreground = (Brush)FindResource("Muted"); Durum.Text = m; }
}
