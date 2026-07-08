using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SSTDigitalRD.Server.Services
{
    public interface IHashService
    {
        string GenerarHash(object datos);
        bool VerificarHash(object datos, string hashEsperado);
    }

    public class HashService : IHashService
    {
        public string GenerarHash(object datos)
        {
            var json = JsonSerializer.Serialize(datos);
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        public bool VerificarHash(object datos, string hashEsperado)
        {
            var hashActual = GenerarHash(datos);
            return string.Equals(hashActual, hashEsperado,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
