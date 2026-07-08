namespace SSTDigitalRD.Shared.DTOs
{
    public class EntregaEPPListDto
    {
        public int Id { get; set; }
        public string NombreTrabajador { get; set; } = "";
        public string CedulaTrabajador { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Cuadrilla { get; set; } = "";
        public string Obra { get; set; } = "";
        public DateTime FechaEntrega { get; set; }
        public bool Firmado { get; set; }
        public string? HashSha256 { get; set; }
        public string EstadoGeneral { get; set; } = "";
        public List<ArticuloEPPDto> Articulos { get; set; } = new();
    }

    public class EntregaEPPDetalleDto
    {
        public int Id { get; set; }
        public string NombreTrabajador { get; set; } = "";
        public string CedulaTrabajador { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Cuadrilla { get; set; } = "";
        public string Obra { get; set; } = "";
        public DateTime FechaEntrega { get; set; }
        public string EntregadoPor { get; set; } = "";
        public bool Firmado { get; set; }
        public string? HashSha256 { get; set; }
        public List<ArticuloEPPDto> Articulos { get; set; } = new();
    }

    public class ArticuloEPPDto
    {
        public int Id { get; set; }
        public string TipoEPP { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Marca { get; set; } = "";
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = "Vigente";
    }

    public class CrearEntregaEPPDto
    {
        public string NombreTrabajador { get; set; } = "";
        public string CedulaTrabajador { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Cuadrilla { get; set; } = "";
        public int ObraId { get; set; }
        public string Obra { get; set; } = "";
        public string EntregadoPor { get; set; } = "";
        public DateTime FechaEntrega { get; set; } = DateTime.UtcNow;
        public string? FirmaBase64 { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public List<ArticuloEPPDto> Articulos { get; set; } = new();
    }

    public class ActualizarArticuloDto
    {
        public int Id { get; set; }
        public string Marca { get; set; } = "";
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = "Vigente";
    }

    public class AlertaEPPDto
    {
        public string NombreTrabajador { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Cuadrilla { get; set; } = "";
        public string TipoEPP { get; set; } = "";
        public int DiasRestantes { get; set; }
        public string Estado { get; set; } = "";
    }

    public class HistorialEPPDto
    {
        public string NombreTrabajador { get; set; } = "";
        public string TipoEPP { get; set; } = "";
        public string Marca { get; set; } = "";
        public DateTime FechaEntrega { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string EntregadoPor { get; set; } = "";
        public string Estado { get; set; } = "";
    }
}
