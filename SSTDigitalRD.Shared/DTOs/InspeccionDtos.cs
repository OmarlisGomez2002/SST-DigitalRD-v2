namespace SSTDigitalRD.Shared.DTOs
{
    // ── Listado ────────────────────────────────────────────────
    public class InspeccionListDto
    {
        public int Id { get; set; }
        public string Area { get; set; } = "";
        public string Obra { get; set; } = "";
        public string Inspector { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = "";
        public bool GpsCapturado { get; set; }
        public bool Firmado { get; set; }
    }

    // ── Detalle completo ───────────────────────────────────────
    public class InspeccionDetalleDto
    {
        public int Id { get; set; }
        public string Area { get; set; } = "";
        public int ObraId { get; set; }
        public string Obra { get; set; } = "";
        public string TipoInspeccion { get; set; } = "";
        public string Inspector { get; set; } = "";
        public string ResponsableArea { get; set; } = "";
        public DateTime FechaInspeccion { get; set; }
        public int CantidadTrabajadores { get; set; }
        public string Descripcion { get; set; } = "";
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public double PrecisionGps { get; set; }
        public bool GpsCapturado { get; set; }
        public string Estado { get; set; } = "";
        public bool Firmado { get; set; }
        public string? HoraFirma { get; set; }
        public string? HashSha256 { get; set; }
        public int CantidadFotos { get; set; }
        public string PlanAccion { get; set; } = "";
        public List<ChecklistItemDto> Items { get; set; } = new();
    }

    // ── Checklist ──────────────────────────────────────────────
    public class ChecklistItemDto
    {
        public int Id { get; set; }
        public string Categoria { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Resultado { get; set; } = "na";
        public string Observacion { get; set; } = "";
    }

    // ── Crear / Editar ─────────────────────────────────────────
    public class CrearInspeccionDto
    {
        public int ObraId { get; set; }
        public string Area { get; set; } = "";
        public string Obra { get; set; } = "";
        public string TipoInspeccion { get; set; } = "";
        public string Inspector { get; set; } = "";
        public string ResponsableArea { get; set; } = "";
        public DateTime FechaInspeccion { get; set; }
        public int CantidadTrabajadores { get; set; }
        public string Descripcion { get; set; } = "";
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public double PrecisionGps { get; set; }
        public string? FirmaBase64 { get; set; }
        public string PlanAccion { get; set; } = "";
        public List<ChecklistItemDto> Items { get; set; } = new();
    }
}
