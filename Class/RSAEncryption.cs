using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

public static class RSAEncryption
{
    private static RSA? rsa;
    private static string? publicKeyPem;
    private static string? privateKeyPem;

    public static async Task GenerarYGuardarClavesAsync()
    {
        rsa = RSA.Create(2048);

        // Exportar claves a PEM
        var pub = rsa.ExportSubjectPublicKeyInfoPem();
        var priv = rsa.ExportPkcs8PrivateKeyPem();

        publicKeyPem = pub;
        privateKeyPem = priv;

        // Guardar clave privada en SecureStorage
        await SecureStorage.SetAsync("clave_privada", privateKeyPem);

        // Enviar clave pública al servidor
        await EnviarClavePublicaAlServidor(pub);
    }

    private static async Task EnviarClavePublicaAlServidor(string clavePublica)
    {
        var usuarioId = "usuario123"; // o tu identificador de sesión real
        using var client = new HttpClient();

        var json = JsonConvert.SerializeObject(new
        {
            UsuarioId = usuarioId,
            ClavePublica = clavePublica
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://TU_SERVIDOR.com/api/clavepublica", content);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Error al enviar la clave pública al servidor: " + response.StatusCode);
    }

    public static async Task<string> ObtenerClavePublicaAsync()
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync("http://localhost:5106/api/rsa/public"); // Cambia el puerto si es distinto
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception("Error al obtener la clave pública.");
            }
        }
    }

    public static string EncriptarConClavePublica(string data, string publicKeyXml)
    {
        using (var rsa = RSA.Create())
        {
            rsa.FromXmlString(publicKeyXml);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var encryptedBytes = rsa.Encrypt(dataBytes, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encryptedBytes);
        }
    }

    public static async Task<string> DesencriptarEnAPIAsync(string encryptedData)
    {
        using (var client = new HttpClient())
        {
            var content = new StringContent($"\"{encryptedData}\"", Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5106/api/rsa/decrypt", content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception("Error al desencriptar en la API.");
            }
        }
    }


}
