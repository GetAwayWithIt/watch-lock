using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WatchLock;

/// <summary>
/// Stores the password as a PBKDF2-SHA256 hash + random salt in %APPDATA%\WatchLock\password.json.
/// The password itself is never written anywhere.
/// </summary>
public static class PasswordStore
{
    private const int Iterations = 200_000;
    private static readonly string FilePath = Path.Combine(AppPaths.Folder, "password.json");

    private sealed record Record(string Salt, string Hash, int Iterations);

    public static bool HasPassword => File.Exists(FilePath);

    public static void Save(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Derive(password, salt, Iterations);
        Directory.CreateDirectory(AppPaths.Folder);
        var record = new Record(Convert.ToBase64String(salt), Convert.ToBase64String(hash), Iterations);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(record));
    }

    public static bool Verify(string password)
    {
        try
        {
            var record = JsonSerializer.Deserialize<Record>(File.ReadAllText(FilePath));
            if (record is null) return false;
            byte[] expected = Convert.FromBase64String(record.Hash);
            byte[] actual = Derive(password, Convert.FromBase64String(record.Salt), record.Iterations);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// v0.2 and earlier ("Ekran Kilidi") kept the hash in %APPDATA%\EkranKilidi\ayarlar.json
    /// with Turkish field names. Carry it over once so existing users keep their password.
    /// </summary>
    /// <returns>true if an old password was migrated.</returns>
    public static bool MigrateFromOldVersion()
    {
        try
        {
            string oldFile = Path.Combine(AppPaths.OldFolder, "ayarlar.json");
            if (HasPassword || !File.Exists(oldFile)) return false;

            var old = JsonNode.Parse(File.ReadAllText(oldFile));
            string? salt = old?["Salt"]?.GetValue<string>();
            string? hash = old?["Hash"]?.GetValue<string>();
            int iterations = old?["Tekrar"]?.GetValue<int>() ?? Iterations;
            if (salt is null || hash is null) return false;

            Directory.CreateDirectory(AppPaths.Folder);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(new Record(salt, hash, iterations)));
            Directory.Delete(AppPaths.OldFolder, recursive: true);
            return true;
        }
        catch
        {
            return false; // if migration fails, the user simply sets a new password
        }
    }

    private static byte[] Derive(string password, byte[] salt, int iterations) =>
        Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, 32);
}
