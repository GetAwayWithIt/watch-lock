<p align="center">
  <img src="assets/logo/logo-128.png" width="96" alt="Ekran Kilidi logosu">
</p>

<h1 align="center">Ekran Kilidi (Watch Lock)</h1>

<p align="center">
  <b>Ekranın görünür kalır, klavye ve fare kilitlenir.</b><br>
  Uzun süren işler çalışırken bilgisayarının başından kalkabilmen için küçük bir Windows uygulaması.
</p>

<p align="center">
  <a href="README.md">English</a> · <b>Türkçe</b>
</p>

---

## Neden?

Bilgisayarında uzun bir iş çalışıyor: bir yapay zekâ ajanı (Claude Code, Codex vb.) kod yazıyor, bir proje derleniyor, büyük bir dosya iniyor. Kalkıp bir kahve almak istiyorsun ama:

- **Win+L** ile kilitlersen ekran kapanır, işin ne durumda olduğunu göremezsin.
- **Açık bırakırsan** biri (çocuk, kedi, ofis arkadaşı) gelip bir tuşa basar, terminale bir şey yazar, pencereyi kapatır.

Ekran Kilidi ikisinin ortası: **ekran olduğu gibi görünür kalır, ama şifreni yazana kadar kimse klavyeye ya da fareye dokunamaz.** Arkadaki işin çalışmaya devam eder, bilgisayar uykuya da geçmez.

## Özellikler

- 🔒 **Görünür kilit:** Ekranın üstüne hafif karartılmış, saydam bir katman gelir. Altındaki her şeyi görmeye devam edersin.
- ⌨️ **Klavye ve fare kilidi:** Tuşlar ve tıklamalar sadece şifre kutusuna gider. Alt+Tab, Win tuşu, Alt+F4, Ctrl+Shift+Esc engellenir.
- 🛡️ **Ctrl+Alt+Del'de Windows kilidine düşer:** Ctrl+Alt+Del'i hiçbir uygulama engelleyemez. Ekran Kilidi bunu fark edince anında gerçek Windows kilidini devreye sokar; bundan sonrası için Windows şifresi gerekir.
- ☕ **Uyku engeli:** Kilitliyken bilgisayar uykuya ya da ekran koruyucuya geçmez.
- 🖥️ **Çoklu monitör:** Bütün ekranlar kilitlenir, şifre kutusu ana ekranda olur.
- ⚡ **Kısayol:** `Ctrl+AltGr+F` ile tek tuşla kilitle.
- 🪶 **Çok hafif:** Arka planda (sistem tepsisinde) yaklaşık **2,5 MB RAM** kullanır, işlemciyi neredeyse hiç kullanmaz.
- 🙈 **Veri toplamaz:** İnternete hiç bağlanmaz. Ayrıntılar aşağıda.

## Kullanım alanları

- Bir yapay zekâ ajanı uzun bir görevde çalışırken ilerlemesini uzaktan izlemek
- Uzun bir derleme, test, render ya da yükleme sürerken masadan kalkmak
- Ekranda bir sunum, pano (dashboard) ya da canlı yayın açıkken kimse dokunmasın istemek
- Evde çocuklar ya da evcil hayvanlar varken bilgisayarı açık bırakmak
- Ofiste kısa bir molada ekranını kurcalanmaya karşı korumak

## İndirme

[**Releases**](../../releases) sayfasından iki sürümden birini indir:

| Dosya | Boyut | Ne zaman? |
|---|---|---|
| `EkranKilidi-<sürüm>-win-x64.exe` | ~62 MB | **Önerilen.** Hiçbir şey kurmadan çalışır. |
| `EkranKilidi-<sürüm>-win-x64-dotnet.exe` | ~240 KB | Bilgisayarında [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) varsa. |

Kurulum yok: exe'yi istediğin bir klasöre koy ve çalıştır.

> **"Windows bilgisayarınızı korudu" uyarısı çıkarsa:** Uygulama imzalı olmadığı için (kod imzalama sertifikası ücretli) Windows SmartScreen uyarı verebilir. **Ek bilgi → Yine de çalıştır** de. Emin olmak istersen kaynak kodu bu repoda; kendin de derleyebilirsin (aşağıda anlatılıyor).

## Nasıl kullanılır?

1. **İlk açılış:** Bir kilit şifresi belirle (en az 4 karakter). Windows şifrenden farklı olabilir.
2. **Kilitle:** Penceredeki **Kilitle** butonuna bas ya da herhangi bir yerdeyken **`Ctrl+AltGr+F`** kısayolunu kullan.
3. **Aç:** Şifreyi yaz, **Enter**'a bas.
4. **Arka planda bırak:** Pencereyi kapatınca uygulama kapanmaz, saatin yanındaki **sistem tepsisine** iner.
   - Simgeye sol tıkla: pencere açılır.
   - Sağ tıkla: Aç / Kilitle / Çıkış menüsü gelir.

### Deneme modu

İlk açılışta **"Deneme modu"** işaretlidir: kilit **60 saniye sonra kendiliğinden açılır.** Uygulamayı tanırken kendini dışarıda bırakma diye konuldu. Nasıl çalıştığını gördükten sonra işareti kaldır; yoksa kilit 1 dakikada açılır.

## Şifremi unuttum

Panik yok, Windows hesabına erişimin olduğu sürece her zaman çıkış yolu var:

1. **Ctrl+Alt+Del**'e bas. Ekran Kilidi hemen Windows kilit ekranına geçer.
2. **Windows şifren ya da PIN'inle** giriş yap.
3. Windows'a geri döndüğünde Ekran Kilidi **kendiliğinden açılır.** Mantığı şu: Windows şifresini bilen kişi zaten bilgisayarın sahibidir.

**Şifreyi sıfırlamak için:** Tepsideki simgeye sağ tıklayıp **Çıkış** de. Sonra şu dosyayı sil:

```
%APPDATA%\EkranKilidi\ayarlar.json
```

Uygulamayı yeniden açınca yeni bir şifre ister. (Şifreyi biliyorsan pencerede **Şifreyi değiştir** bağlantısını kullanabilirsin.)

## Gizlilik ve veri toplama

**Ekran Kilidi hiçbir veri toplamaz ve hiçbir yere göndermez.**

- **İnternet yok:** Kodda tek bir ağ bağlantısı yok. Analitik, reklam, güncelleme kontrolü, "telemetri" yok.
- **Şifren düz metin olarak saklanmaz:** Sadece şifrenin tek yönlü özeti (PBKDF2-SHA256, 200.000 tekrar, rastgele salt) saklanır. Bu özetten şifre geri çıkarılamaz.
- **Bilgisayarında tuttuğu dosyalar** (hepsi `%APPDATA%\EkranKilidi\` klasöründe):
  - `ayarlar.json`: şifre özeti ve salt
  - `tercihler.json`: deneme modu açık mı, tepsi bildirimi gösterildi mi
- **Tuş takibi hakkında:** Kilidin çalışması için uygulama klavyeyi alt seviyeden dinler (keylogger'ların kullandığı Windows özelliğinin aynısı). Kilit açıkken sadece `Ctrl+AltGr+F` kombinasyonuna bakar; kilitliyken de yasak tuşları engeller. **Basılan tuşlar hiçbir yere yazılmaz, kaydedilmez ya da gönderilmez.** Bunu doğrulamak istersen ilgili kod [`Tepsi.cs`](src/EkranKilidi/Tepsi.cs) ve [`LockSession.cs`](src/EkranKilidi/LockSession.cs) içinde, birkaç düzine satır.
- **Kayıt defteri yok, otomatik başlatma yok.** Kaldırmak için exe'yi ve `%APPDATA%\EkranKilidi` klasörünü silmen yeterli.

## Güvenlik: ne yapar, ne yapamaz?

Ekran Kilidi **pratik bir kilittir, bir güvenlik ürünü değildir.** Amacı, bilgisayarının başına geçen birinin "yanlışlıkla ya da merakla" bir şeyleri bozmasını önlemek. Bilgisayarına fiziksel erişimi olan kararlı birine karşı koruma için Windows'un kendi kilidini (Win+L) ve disk şifrelemeyi (BitLocker) kullan.

Bilmen gerekenler:

- **Ctrl+Alt+Del engellenemez:** Bu Windows'un bilinçli bir tasarımı. Ekran Kilidi bu durumda gerçek Windows kilidine geçer, yani kilit kırılmaz, sadece Windows'un kilidine devredilir. Ekran o noktada görünmez olur ama işin çalışmaya devam eder.
- **Güç tuşu:** Bilgisayarın fiziksel güç tuşuna basılmasını hiçbir yazılım engelleyemez.
- **Yönetici izni (UAC) pencereleri:** Kilitliyken bir program yönetici izni isterse Windows güvenli ekrana geçer. Ekran Kilidi bunu da Ctrl+Alt+Del gibi algılar ve Windows kilidine geçer.
- **Yanlış şifre denemeleri:** Her yanlış denemeden sonra bekleme süresi artar (en fazla 2 saniye), ama henüz deneme sınırı yok. Kısa bir PIN yerine biraz uzun bir şifre seç.
- **Antivirüs uyarısı:** Klavyeyi alt seviyeden dinlediği için bazı antivirüs programları uygulamayı şüpheli bulabilir. Kodun tamamı açık; dilersen kendin derle.

## Performans

| Durum | RAM (Görev Yöneticisi) | İşlemci |
|---|---|---|
| Tepside beklerken | ~2,5 MB | ~%0 |
| Pencere açıkken | ~25-80 MB* | ~%0 |

\* Bağımsız (62 MB) sürümde pencere açıkken daha yüksek. Pencere ya da kilit ekranı kapandıktan birkaç saniye sonra uygulama belleği geri bırakır ve tekrar ~2-3 MB'a iner.

Bunu sağlayan: pencere kapanınca tamamen bellekten atılır, çöp toplayıcı çalıştırılır ve kullanılmayan bellek Windows'a geri verilir. Tepsi simgesi de ağır bir kütüphane yüklemeden, doğrudan Windows API'si ile yapıldı.

## Kaynak koddan derleme

Gerekenler: Windows 10/11, [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
git clone https://github.com/GetAwayWithIt/watch-lock.git
cd watch-lock
dotnet run --project src/EkranKilidi
```

Tek exe çıkarmak için:

```powershell
# Küçük (.NET 10 Desktop Runtime gerektirir)
dotnet publish src/EkranKilidi -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugType=none -o publish

# Bağımsız (hiçbir şey gerektirmez)
dotnet publish src/EkranKilidi -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none -o publish/tam
```

Logo kodla çiziliyor: `tools/ikon-ciz.ps1` → `assets/logo/` ve `src/EkranKilidi/app.ico`.

## Yol haritası

- [ ] Belli sayıda yanlış şifreden sonra Windows kilidine geçme
- [ ] Yanlış deneme günlüğü (saatleriyle)
- [ ] Arayüz için İngilizce dil seçeneği
- [ ] Windows ile birlikte başlatma seçeneği
- [ ] Ajan işi bitince bildirim

## Lisans

[MIT](LICENSE): dilediğin gibi kullan, değiştir, dağıt.
