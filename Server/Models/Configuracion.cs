using System.ComponentModel.DataAnnotations;

namespace SSTDigitalRD.Server.Models
{
    public class ConfiguracionEmpresa
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string RazonSocial { get; set; } = "";

        [MaxLength(20)]
        public string RNC { get; set; } = "";

        [MaxLength(200)]
        public string Sector { get; set; } = "";

        [MaxLength(150)]
        public string ResponsableSST { get; set; } = "";

        [MaxLength(20)]
        public string Telefono { get; set; } = "";

        [MaxLength(150)]
        public string Correo { get; set; } = "";

        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }

    public class ObraActiva
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Nombre { get; set; } = "";

        [MaxLength(300)]
        public string Direccion { get; set; } = "";

        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public int RadioGeofencing { get; set; } = 100;

        public bool Activa { get; set; } = true;

        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }

    public class NotificacionConfig
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Titulo { get; set; } = "";

        [MaxLength(300)]
        public string Descripcion { get; set; } = "";

        public bool Activa { get; set; } = true;

        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }

    public class UsuarioSistema
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = "";

        [Required, MaxLength(150)]
        public string Correo { get; set; } = "";

        [Required, MaxLength(50)]
        public string Rol { get; set; } = "Prevencionista";

        [MaxLength(100)]
        public string Cuadrilla { get; set; } = "";

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // ── Autenticación ──────────────────────────────────────
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public DateTime? UltimoAcceso { get; set; }
    }

    public class TipoInspeccion
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = "";

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }

    public class Cuadrilla
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = "";

        public bool Activa { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }

    public class TipoCharla
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = "";

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }

    public class CargoEmpleado
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = "";

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }

    public class ItemChecklist
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Categoria { get; set; } = "";
        [Required, MaxLength(300)]
        public string Descripcion { get; set; } = "";
        public bool Activo { get; set; } = true;
        public int Orden { get; set; } = 0;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }

    public class TipoEPP
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = "";
        [MaxLength(100)]
        public string Categoria { get; set; } = "";
        [MaxLength(50)]
        public string Icono { get; set; } = "ti-hardhat";
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
