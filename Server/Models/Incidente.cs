using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSTDigitalRD.Server.Models
{
    public class Incidente
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(300)]
        public string Descripcion { get; set; } = "";

        [Required, MaxLength(100)]
        public string Tipo { get; set; } = "";

        [Required, MaxLength(200)]
        public string Area { get; set; } = "";

        [Required, MaxLength(200)]
        public string Obra { get; set; } = "";

        [MaxLength(150)]
        public string Afectado { get; set; } = "";

        [MaxLength(150)]
        public string Inspector { get; set; } = "";

        [MaxLength(100)]
        public string AtencionMedica { get; set; } = "";

        [MaxLength(500)]
        public string Testigos { get; set; } = "";

        public DateTime FechaIncidente { get; set; }
        public int DiasPerdidos { get; set; }

        // GPS
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public double PrecisionGps { get; set; }
        public bool GpsCapturado { get; set; }

        // Fotos
        public int CantidadFotos { get; set; }

        // Notificación MTRAB
        public bool NotificarMTRAB { get; set; }
        public bool NotificadoMTRAB { get; set; }

        // Estado
        [Required, MaxLength(50)]
        public string Estado { get; set; } = "En seguimiento";

        // Firma
        public bool Firmado { get; set; }
        public string? FirmaBase64 { get; set; }

        // SHA-256
        [MaxLength(64)]
        public string? HashSha256 { get; set; }
        public int ObraId { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public ICollection<AccionCorrectiva> AccionesCorrectivas { get; set; }
            = new List<AccionCorrectiva>();
    }

    public class AccionCorrectiva
    {
        [Key]
        public int Id { get; set; }

        public int IncidenteId { get; set; }

        [Required, MaxLength(500)]
        public string Descripcion { get; set; } = "";

        [MaxLength(150)]
        public string Responsable { get; set; } = "";

        public DateTime FechaLimite { get; set; }

        [MaxLength(50)]
        public string Estado { get; set; } = "Pendiente";

        [ForeignKey(nameof(IncidenteId))]
        public Incidente? Incidente { get; set; }
    }
}
