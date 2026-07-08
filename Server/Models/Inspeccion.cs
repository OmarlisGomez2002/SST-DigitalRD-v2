using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSTDigitalRD.Server.Models;

public class Inspeccion
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Area { get; set; } = "";

    [Required, MaxLength(200)]
    public string Obra { get; set; } = "";

    [Required, MaxLength(100)]
    public string TipoInspeccion { get; set; } = "";

    [Required, MaxLength(150)]
    public string Inspector { get; set; } = "";

    [Required, MaxLength(150)]
    public string ResponsableArea { get; set; } = "";

    public DateTime FechaInspeccion { get; set; }

    public int CantidadTrabajadores { get; set; }

    [MaxLength(1000)]
    public string Descripcion { get; set; } = "";

    // GPS
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public double PrecisionGps { get; set; }
    public bool GpsCapturado { get; set; }

    // Estado
    [Required, MaxLength(50)]
    public string Estado { get; set; } = "En proceso";

    // Firma digital
    public bool Firmado { get; set; }
    public string? FirmaBase64 { get; set; }
    public string? HoraFirma { get; set; }

    // SHA-256
    [MaxLength(64)]
    public string? HashSha256 { get; set; }

    // Fotos
    public int CantidadFotos { get; set; }

    // Plan de acción
    [MaxLength(500)]
    public string PlanAccion { get; set; } = "";

    // Auditoría
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
}

public class ChecklistItem
{
    [Key]
    public int Id { get; set; }

    public int InspeccionId { get; set; }

    [Required, MaxLength(100)]
    public string Categoria { get; set; } = "";

    [Required, MaxLength(300)]
    public string Descripcion { get; set; } = "";

    [Required, MaxLength(10)]
    public string Resultado { get; set; } = "na";

    [MaxLength(500)]
    public string Observacion { get; set; } = "";

    // Navegación
    [ForeignKey(nameof(InspeccionId))]
    public Inspeccion? Inspeccion { get; set; }
}