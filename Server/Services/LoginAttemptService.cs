using Microsoft.Extensions.Caching.Memory;

namespace SSTDigitalRD.Server.Services
{
    public interface ILoginAttemptService
    {
        bool EstaBloquedo(string correo);
        void RegistrarIntento(string correo);
        void LimpiarIntentos(string correo);
    }

    public class LoginAttemptService : ILoginAttemptService
    {
        private readonly IMemoryCache _cache;
        private const int MaxIntentos = 5;
        private const int BloqueoMinutos = 15;

        public LoginAttemptService(IMemoryCache cache)
            => _cache = cache;

        public bool EstaBloquedo(string correo)
        {
            var key = $"blocked_{correo.ToLower()}";
            return _cache.TryGetValue(key, out _);
        }

        public void RegistrarIntento(string correo)
        {
            var keyIntentos = $"attempts_{correo.ToLower()}";
            var keyBloqueo = $"blocked_{correo.ToLower()}";

            var intentos = _cache.GetOrCreate(keyIntentos, e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(BloqueoMinutos);
                return 0;
            });

            intentos++;
            _cache.Set(keyIntentos, intentos, TimeSpan.FromMinutes(BloqueoMinutos));

            if (intentos >= MaxIntentos)
            {
                _cache.Set(keyBloqueo, true, TimeSpan.FromMinutes(BloqueoMinutos));
                _cache.Remove(keyIntentos);
            }
        }

        public void LimpiarIntentos(string correo)
        {
            _cache.Remove($"attempts_{correo.ToLower()}");
            _cache.Remove($"blocked_{correo.ToLower()}");
        }
    }
}
