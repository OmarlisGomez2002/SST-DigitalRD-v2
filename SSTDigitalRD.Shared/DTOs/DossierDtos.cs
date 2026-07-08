using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTDigitalRD.Shared.DTOs
{
    public class DossierResumenDto
    {
        public int ReportesGenerados { get; set; }
        public int TotalReportes { get; set; }
        public int PorcentajeCompletitud { get; set; }
        public string UltimaGeneracion { get; set; } = "";
        public int DiasProximaEntrega { get; set; }
        public List<ReporteStatusDto> Reportes { get; set; } = new();
    }

    public class ReporteStatusDto
    {
        public string Letra { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string BaseLegal { get; set; } = "";
        public string Frecuencia { get; set; } = "";
        public string Detalle { get; set; } = "";
        public string Periodo { get; set; } = "";
        public string RegistrosIncluidos { get; set; } = "";
        public string Estado { get; set; } = "pendiente";
        public int Completitud { get; set; }
        public string HashSha256 { get; set; } = "";
        public string MensajePendiente { get; set; } = "";
        public string FechaGeneracion { get; set; } = "";
    }

    public class GenerarReporteDto
    {
        public string Letra { get; set; } = "";
        public string Periodo { get; set; } = "";
    }
}
