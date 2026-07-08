using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;
using SSTDigitalRD.Shared.DTOs;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;


namespace SSTDigitalRD.Client
{
    public class SSTAuthStateProvider
        : AuthenticationStateProvider
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _storage;

        public SSTAuthStateProvider(
            HttpClient http, ILocalStorageService storage)
        {
            _http = http;
            _storage = storage;
        }

        public override async Task<AuthenticationState>
            GetAuthenticationStateAsync()
        {
            var token = await _storage.GetItemAsync<string>("token");

            if (string.IsNullOrEmpty(token))
                return EstadoAnonimo();

            // Verificar expiración
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                await _storage.RemoveItemAsync("token");
                return EstadoAnonimo();
            }

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var identity = new ClaimsIdentity(
                jwt.Claims, "jwt");

            return new AuthenticationState(
                new ClaimsPrincipal(identity));
        }

        public async Task Login(LoginResponseDto response)
        {
            await _storage.SetItemAsync("token", response.Token);
            await _storage.SetItemAsync("nombre", response.Nombre);
            await _storage.SetItemAsync("rol", response.Rol);
            await _storage.SetItemAsync("usuarioId", response.UsuarioId.ToString());

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer", response.Token);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(response.Token);
            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(
                Task.FromResult(
                    new AuthenticationState(user)));
        }

        public async Task Logout()
        {
            await _storage.RemoveItemAsync("token");
            await _storage.RemoveItemAsync("nombre");
            await _storage.RemoveItemAsync("rol");
            await _storage.RemoveItemAsync("usuarioId");

            _http.DefaultRequestHeaders.Authorization = null;

            NotifyAuthenticationStateChanged(
                Task.FromResult(EstadoAnonimo()));
        }

        private static AuthenticationState EstadoAnonimo()
            => new(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}
