using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSTDigitalRD.Server.Models
{
    public class Charla
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Tema { get; set; } = "";

        [Required, MaxLength(150)]
        public string Instructor { get; set; } = "";

        [Required, MaxLength(200)]
        public string Obra { get; set; } = "";

        [MaxLength(100)]
        public string Cuadrilla { get; set; } = "";

        public DateTime FechaCharla { get; set; }
        public int DuracionMinutos { get; set; }
        public int TotalAsistentes { get; set; }

        // GPS
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public double PrecisionGps { get; set; }
        public bool GpsCapturado { get; set; }

        // Foto grupal
        public bool FotoCapturada { get; set; }
        public string? FotoBase64 { get; set; }
        public int ConteoFacial { get; set; }

        // Firma
        public bool Firmado { get; set; }
        public string? FirmaBase64 { get; set; }
        public string? HoraFirma { get; set; }

        // SHA-256
        [MaxLength(64)]
        public string? HashSha256 { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public ICollection<AsistenteCharla> Asistentes { get; set; }
            = new List<AsistenteCharla>();
    }

    public class AsistenteCharla
    {
        [Key]
        public int Id { get; set; }

        public int CharlaId { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = "";

        [MaxLength(20)]
        public string Cedula { get; set; } = "";

        [MaxLength(100)]
        public string Cargo { get; set; } = "";

        public bool Presente { get; set; } = true;
        public string? FirmaBase64 { get; set; }

        [ForeignKey(nameof(CharlaId))]
        public Charla? Charla { get; set; }
    }
}
