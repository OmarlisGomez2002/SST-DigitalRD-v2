using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSTDigitalRD.Server.Models
{
    public class EntregaEPP
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string NombreTrabajador { get; set; } = "";

        [MaxLength(20)]
        public string CedulaTrabajador { get; set; } = "";

        [Required, MaxLength(100)]
        public string Cargo { get; set; } = "";

        [Required, MaxLength(100)]
        public string Cuadrilla { get; set; } = "";

        [Required, MaxLength(200)]
        public string Obra { get; set; } = "";

        public DateTime FechaEntrega { get; set; } = DateTime.UtcNow;

        [MaxLength(150)]
        public string EntregadoPor { get; set; } = "";

        // Firma del trabajador receptor
        public bool Firmado { get; set; }
        public string? FirmaBase64 { get; set; }

        // SHA-256
        [MaxLength(64)]
        public string? HashSha256 { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public ICollection<ArticuloEPP> Articulos { get; set; }
            = new List<ArticuloEPP>();
    }

    public class ArticuloEPP
    {
        [Key]
        public int Id { get; set; }

        public int EntregaEPPId { get; set; }

        [Required, MaxLength(100)]
        public string TipoEPP { get; set; } = "";

        [MaxLength(100)]
        public string Categoria { get; set; } = "";

        [MaxLength(100)]
        public string Marca { get; set; } = "";

        public DateTime FechaVencimiento { get; set; }

        [Required, MaxLength(50)]
        public string Estado { get; set; } = "Vigente";

        [ForeignKey(nameof(EntregaEPPId))]
        public EntregaEPP? EntregaEPP { get; set; }
    }
}
