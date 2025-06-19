using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SintomApp.Class; // Donde está tu clase RSAKeys

public static class RSAHelper
{
    private static RSAKeys? _keys;

    private static void EnsureKeysLoaded()
    {
        if (_keys == null)
        {
            string jsonPath = Path.Combine(AppContext.BaseDirectory, "rsa_keys.json");

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("No se encontró el archivo de claves RSA.", jsonPath);

            string json = File.ReadAllText(jsonPath);

            _keys = JsonSerializer.Deserialize<RSAKeys>(json);

            if (_keys == null)
                throw new Exception("No se pudo deserializar el archivo rsa_keys.json");

            _keys.PublicKey = _keys.PublicKey.Replace("\\n", "\n").Replace("\\r", "\r");
            _keys.PrivateKey = _keys.PrivateKey.Replace("\\n", "\n").Replace("\\r", "\r");
        }
    }


    public static byte[] EncryptToBytes(string plainText)
    {
        EnsureKeysLoaded();

        using var rsa = RSA.Create();
        rsa.ImportFromPem(_keys.PublicKey);

        var data = Encoding.UTF8.GetBytes(plainText);
        return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
    }

    public static string DecryptFromBytes(byte[] encryptedText)
    {
        EnsureKeysLoaded();

        using var rsa = RSA.Create();

        string privateKeyPem = _keys.PrivateKey;

        // Limpia encabezados y pies
        var base64 = privateKeyPem
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\r", "")
            .Replace("\n", "")
            .Trim();

        byte[] keyBytes = Convert.FromBase64String(base64);

        rsa.ImportPkcs8PrivateKey(keyBytes, out _);

        var decrypted = rsa.Decrypt(encryptedText, RSAEncryptionPadding.Pkcs1);
        return Encoding.UTF8.GetString(decrypted);
    }

}
