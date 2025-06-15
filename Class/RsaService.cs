using System.Security.Cryptography;
using System.Text;

namespace SintomApp.Class
{
    public class RsaService
    {
        private readonly RSA _rsa;

        public RsaService(string clavePublicaPem)
        {
            _rsa = RSA.Create();
            _rsa.ImportFromPem(clavePublicaPem.ToCharArray());
        }

        public string Cifrar(string texto)
        {
            byte[] datos = Encoding.UTF8.GetBytes(texto);
            byte[] cifrado = _rsa.Encrypt(datos, RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(cifrado);
        }
    }
}
