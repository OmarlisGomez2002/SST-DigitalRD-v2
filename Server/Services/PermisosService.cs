using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace SSTDigitalRD.Server.Services
{
    public class PermisosService
    {
        private readonly AuthenticationStateProvider _auth;
        private ClaimsPrincipal? _user;

        public PermisosService(AuthenticationStateProvider auth)
            => _auth = auth;

        private async Task<ClaimsPrincipal> GetUser()
        {
            if (_user is not null) return _user;
            var state = await _auth.GetAuthenticationStateAsync();
            _user = state.User;
            return _user;
        }

        public async Task<string> GetRol()
        {
            var user = await GetUser();
            return user.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        public async Task<bool> EsAdmin()
            => await GetRol() == "Administrador";

        public async Task<bool> EsPrevencionista()
            => await GetRol() == "Prevencionista";

        public async Task<bool> EsSupervisor()
            => await GetRol() == "Supervisor";

        public async Task<bool> PuedeCrear(string modulo)
        {
            var rol = await GetRol();
            return modulo switch
            {
                "Inspecciones" => rol is "Administrador" or "Prevencionista",
                "Charlas" => rol is "Administrador" or "Prevencionista",
                "EPP" => rol is "Administrador" or "Prevencionista",
                "Incidentes" => rol is "Administrador" or "Prevencionista",
                "Empleados" => rol is "Administrador" or "Prevencionista",
                "Configuracion" => rol == "Administrador",
                _ => rol is "Administrador" or "Prevencionista"
            };
        }

        public async Task<bool> PuedeEditar(string modulo)
            => await PuedeCrear(modulo);

        public async Task<bool> PuedeEliminar(string modulo)
            => await GetRol() == "Administrador";

        public async Task<bool> TieneAcceso(string modulo)
        {
            var rol = await GetRol();
            return modulo switch
            {
                "Configuracion" => rol == "Administrador",
                "Dossier" => rol is "Administrador" or "Prevencionista",
                "Empleados" => rol is "Administrador" or "Prevencionista",
                "VisionObra" => rol is "Administrador" or "Prevencionista",
                _ => true
            };
        }

        public async Task<bool> PuedeEjecutarModelo()
        {
            var rol = await GetRol();
            return rol is "Administrador" or "Prevencionista";
        }
    }
}
