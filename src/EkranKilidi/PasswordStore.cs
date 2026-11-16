using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EkranKilidi;

/// <summary>Şifreyi PBKDF2 hash + salt olarak %APPDATA%\EkranKilidi\ayarlar.json içinde tutar.</summary>
public static class PasswordStore
{
    private const int Tekrar = 200_000;
    private static readonly string Klasor =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EkranKilidi");
    private static readonly string Dosya = Path.Combine(Klasor, "ayarlar.json");

    private record Kayit(string Salt, string Hash, int Tekrar);

    public static bool SifreVar => File.Exists(Dosya);

    public static void Kaydet(string sifre)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Hesapla(sifre, salt, Tekrar);
        Directory.CreateDirectory(Klasor);
        var kayit = new Kayit(Convert.ToBase64String(salt), Convert.ToBase64String(hash), Tekrar);
        File.WriteAllText(Dosya, JsonSerializer.Serialize(kayit));
    }

    public static bool Dogrula(string sifre)
    {
        try
        {
            var kayit = JsonSerializer.Deserialize<Kayit>(File.ReadAllText(Dosya));
            if (kayit is null) return false;
            byte[] beklenen = Convert.FromBase64String(kayit.Hash);
            byte[] gelen = Hesapla(sifre, Convert.FromBase64String(kayit.Salt), kayit.Tekrar);
            return CryptographicOperations.FixedTimeEquals(beklenen, gelen);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] Hesapla(string sifre, byte[] salt, int tekrar) =>
        Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(sifre), salt, tekrar, HashAlgorithmName.SHA256, 32);
}
