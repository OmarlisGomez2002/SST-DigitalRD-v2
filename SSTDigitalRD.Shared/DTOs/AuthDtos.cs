using System.ComponentModel.DataAnnotations;

namespace SSTDigitalRD.Shared.DTOs
{
    public class LoginDto
    {
        [Required, EmailAddress, MaxLength(150)]
        public string Correo { get; set; } = "";

        [Required, MinLength(8), MaxLength(100)]
        public string Password { get; set; } = "";
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Rol    { get; set; } = "";
        public int UsuarioId { get; set; }
        public bool Requiere2FA { get; set; } = false;
    }

    public class CambiarPasswordDto
    {
        public string PasswordActual { get; set; } = "";
        public string PasswordNuevo { get; set; } = "";
    }

    public class EditarUsuarioDto
    {
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Rol { get; set; } = "";
        public string Cuadrilla { get; set; } = "";
        public bool Activo { get; set; } = true;
    }

    public class CambiarPasswordAdminDto
    {
        public string PasswordNuevo { get; set; } = "";
    }

    public class CrearUsuarioConPasswordDto
    {
        [Required, MaxLength(150)]
        public string Nombre { get; set; } = "";
        [Required, EmailAddress, MaxLength(150)]
        public string Correo { get; set; } = "";
        [Required, MaxLength(50)]
        public string Rol { get; set; } = "";
        [MaxLength(100)]
        public string Cuadrilla { get; set; } = "";
        [Required, MinLength(8), MaxLength(100)]
        public string Password { get; set; } = "";
    }
}
