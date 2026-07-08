namespace SSTDigitalRD.Shared.DTOs
{
    public class IncidenteListDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Area { get; set; } = "";
        public string Obra { get; set; } = "";
        public int ObraId { get; set; }
        public string Afectado { get; set; } = "";
        public string Inspector { get; set; } = "";
        public DateTime Fecha { get; set; }
        public int DiasPerdidos { get; set; }
        public bool GpsCapturado { get; set; }
        public bool Firmado { get; set; }
        public string Estado { get; set; } = "";
        public string? HashSha256 { get; set; }
    }

    public class IncidenteDetalleDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Area { get; set; } = "";
        public string Obra { get; set; } = "";
        public string Afectado { get; set; } = "";
        public string Inspector { get; set; } = "";
        public string AtencionMedica { get; set; } = "";
        public string Testigos { get; set; } = "";
        public DateTime Fecha { get; set; }
        public int DiasPerdidos { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public double PrecisionGps { get; set; }
        public bool GpsCapturado { get; set; }
        public int CantidadFotos { get; set; }
        public bool NotificarMTRAB { get; set; }
        public bool NotificadoMTRAB { get; set; }
        public string Estado { get; set; } = "";
        public bool Firmado { get; set; }
        public string? HashSha256 { get; set; }
        public List<AccionCorrectivaDto> AccionesCorrectivas { get; set; } = new();
    }

    public class AccionCorrectivaDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = "";
        public string Responsable { get; set; } = "";
        public DateTime FechaLimite { get; set; }
        public string Estado { get; set; } = "Pendiente";
    }

    public class CrearIncidenteDto
    {
        public string Descripcion { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Area { get; set; } = "";
        public int ObraId { get; set; }
        public string Obra { get; set; } = "";
        public string Afectado { get; set; } = "";
        public string Inspector { get; set; } = "";
        public string AtencionMedica { get; set; } = "";
        public string Testigos { get; set; } = "";
        public DateTime Fecha { get; set; }
        public int DiasPerdidos { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public double PrecisionGps { get; set; }
        public int CantidadFotos { get; set; }
        public bool NotificarMTRAB { get; set; }
        public string? FirmaBase64 { get; set; }
        public List<AccionCorrectivaDto> AccionesCorrectivas { get; set; } = new();
    }
}
