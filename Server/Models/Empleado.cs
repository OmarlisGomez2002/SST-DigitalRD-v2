using System.ComponentModel.DataAnnotations;

namespace SSTDigitalRD.Server.Models
{
    public class Empleado
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = "";

        [Required, MaxLength(20)]
        public string Cedula { get; set; } = "";

        public DateTime FechaNacimiento { get; set; }

        [MaxLength(20)]
        public string Telefono { get; set; } = "";

        [MaxLength(150)]
        public string Correo { get; set; } = "";

        [MaxLength(300)]
        public string Direccion { get; set; } = "";

        [Required, MaxLength(100)]
        public string Cargo { get; set; } = "";

        [Required, MaxLength(100)]
        public string Cuadrilla { get; set; } = "";

        [Required, MaxLength(200)]
        public string Obra { get; set; } = "";

        public DateTime FechaIngreso { get; set; }

        [MaxLength(50)]
        public string TipoContrato { get; set; } = "Por obra";

        [MaxLength(50)]
        public string NumeroTSS { get; set; } = "";

        [Required, MaxLength(20)]
        public string Estado { get; set; } = "Activo";

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }
}
