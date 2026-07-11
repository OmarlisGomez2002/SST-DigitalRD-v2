using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Server.Models;
using SSTDigitalRD.Server.Services;
using SSTDigitalRD.Shared.DTOs;

namespace SSTDigitalRD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAuthService _auth;

        public AuthController(AppDbContext db, IAuthService auth)
        {
            _db = db;
            _auth = auth;
        }

        // ── POST /api/auth/login ───────────────────────────────
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>>
            Login([FromBody] LoginDto dto)
        {
            var usuario = await _db.UsuariosSistema
                .FirstOrDefaultAsync(x =>
                    x.Correo == dto.Correo && x.Activo);

            if (usuario is null)
                return Unauthorized(new
                {
                    error = "Correo o contraseña incorrectos."
                });

            if (!_auth.VerificarPassword(
                    dto.Password,
                    usuario.PasswordHash,
                    usuario.PasswordSalt))
                return Unauthorized(new
                {
                    error = "Correo o contraseña incorrectos."
                });

            usuario.UltimoAcceso = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new LoginResponseDto
            {
                Token = _auth.GenerarToken(usuario),
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                UsuarioId = usuario.Id
            });
        }

        // ── POST /api/auth/seed ────────────────────────────────
        // Solo para desarrollo — crea usuario admin inicial
        [HttpPost("seed")]
        public async Task<IActionResult> SeedAdmin()
        {
            var existe = await _db.UsuariosSistema
                .AnyAsync(x => x.Correo == "admin@sst.do");

            if (existe)
                return Conflict(new
                {
                    error = "El usuario admin ya existe."
                });

            _auth.CrearPasswordHash(
                "Admin2026!", out var hash, out var salt);

            var usuarios = new List<UsuarioSistema>
            {
                new()
                {
                    Nombre       = "Administrador SST",
                    Correo       = "admin@sst.do",
                    Rol          = "Administrador",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Activo       = true
                },
                new()
                {
                    Nombre       = "Ramón Gómez",
                    Correo       = "ramon@sst.do",
                    Rol          = "Prevencionista",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Activo       = true
                },
                new()
                {
                    Nombre       = "Supervisor Obra",
                    Correo       = "supervisor@sst.do",
                    Rol          = "Supervisor",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Activo       = true
                }
            };

            _db.UsuariosSistema.AddRange(usuarios);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Usuarios seed creados.",
                credenciales = new[]
                {
                    "admin@sst.do / Admin2026!",
                    "ramon@sst.do / Admin2026!",
                    "supervisor@sst.do / Admin2026!"
                }
            });
        }

        // ── PUT /api/auth/cambiar-password ────────────────────
        [HttpPut("cambiar-password/{id:int}")]
        public async Task<IActionResult> CambiarPassword(
            int id, [FromBody] CambiarPasswordDto dto)
        {
            var usuario = await _db.UsuariosSistema.FindAsync(id);
            if (usuario is null) return NotFound();

            if (!_auth.VerificarPassword(
                    dto.PasswordActual,
                    usuario.PasswordHash,
                    usuario.PasswordSalt))
                return BadRequest(new
                {
                    error = "La contraseña actual es incorrecta."
                });

            _auth.CrearPasswordHash(
                dto.PasswordNuevo,
                out var hash, out var salt);

            usuario.PasswordHash = hash;
            usuario.PasswordSalt = salt;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // ── PUT /api/auth/usuarios/{id} ───────────────────────────
        [HttpPut("usuarios/{id:int}")]
        public async Task<IActionResult> EditarUsuario(
            int id, [FromBody] EditarUsuarioDto dto)
        {
            var usuario = await _db.UsuariosSistema.FindAsync(id);
            if (usuario is null) return NotFound();

            // Verificar correo duplicado en otro usuario
            var correoExiste = await _db.UsuariosSistema
                .AnyAsync(x => x.Correo == dto.Correo && x.Id != id);
            if (correoExiste)
                return Conflict(new
                {
                    error = $"El correo {dto.Correo} ya está en uso."
                });

            usuario.Nombre = dto.Nombre;
            usuario.Correo = dto.Correo;
            usuario.Rol = dto.Rol;
            usuario.Cuadrilla = dto.Cuadrilla;
            usuario.Activo = dto.Activo;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── PUT /api/auth/usuarios/{id}/reset-password ────────────
        // Solo para Administrador — cambia contraseña sin verificar la actual
        [HttpPut("usuarios/{id:int}/reset-password")]
        public async Task<IActionResult> ResetPassword(
            int id, [FromBody] CambiarPasswordAdminDto dto)
        {
            var usuario = await _db.UsuariosSistema.FindAsync(id);
            if (usuario is null) return NotFound();

            _auth.CrearPasswordHash(
                dto.PasswordNuevo, out var hash, out var salt);

            usuario.PasswordHash = hash;
            usuario.PasswordSalt = salt;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── POST /api/auth/crear-usuario ──────────────────────────
        [HttpPost("crear-usuario")]
        public async Task<ActionResult<UsuarioSistemaDto>> CrearUsuario(
            [FromBody] CrearUsuarioConPasswordDto dto)
        {
            var existe = await _db.UsuariosSistema
                .AnyAsync(x => x.Correo == dto.Correo);

            if (existe)
                return Conflict(new
                {
                    error = $"Ya existe un usuario con el correo {dto.Correo}."
                });

            _auth.CrearPasswordHash(
                dto.Password, out var hash, out var salt);

            var usuario = new UsuarioSistema
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                Rol = dto.Rol,
                Cuadrilla = dto.Cuadrilla,
                PasswordHash = hash,
                PasswordSalt = salt,
                Activo = true
            };

            _db.UsuariosSistema.Add(usuario);
            await _db.SaveChangesAsync();

            return Ok(new UsuarioSistemaDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Cuadrilla = usuario.Cuadrilla,
                Activo = usuario.Activo
            });
        }
    }

}
