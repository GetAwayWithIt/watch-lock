using System.IO;
using System.Text.Json;

namespace EkranKilidi;

/// <summary>Küçük kullanıcı tercihleri: %APPDATA%\EkranKilidi\tercihler.json</summary>
public sealed class Tercihler
{
    public bool DenemeModu { get; set; } = true;
    public bool TepsiIpucuGosterildi { get; set; }

    private static readonly string Dosya = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EkranKilidi", "tercihler.json");

    public static Tercihler Yukle()
    {
        try
        {
            if (File.Exists(Dosya))
                return JsonSerializer.Deserialize<Tercihler>(File.ReadAllText(Dosya)) ?? new Tercihler();
        }
        catch { /* bozuk dosya: varsayılanla devam */ }
        return new Tercihler();
    }

    public void Kaydet()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Dosya)!);
            File.WriteAllText(Dosya, JsonSerializer.Serialize(this));
        }
        catch { /* tercih kaydedilemezse önemli değil */ }
    }
}
