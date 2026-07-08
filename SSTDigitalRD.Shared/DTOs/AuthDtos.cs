using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTDigitalRD.Shared.DTOs
{
    public class LoginDto
    {
        public string Correo { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Rol    { get; set; } = "";
        public int UsuarioId { get; set; }
    }

    public class CambiarPasswordDto
    {
        public string PasswordActual { get; set; } = "";
        public string PasswordNuevo { get; set; } = "";
    }
}
