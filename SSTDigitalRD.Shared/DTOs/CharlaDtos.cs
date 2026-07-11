using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTDigitalRD.Shared.DTOs
{
    public class CharlaListDto
    {
        public int Id { get; set; }
        public string Tema { get; set; } = "";
        public string Instructor { get; set; } = "";
        public string Obra { get; set; } = "";
        public string Cuadrilla { get; set; } = "";
        public DateTime FechaCharla { get; set; }
        public int DuracionMinutos { get; set; }
        public int TotalAsistentes { get; set; }
        public int AsistentesPresentes { get; set; }
        public bool GpsCapturado { get; set; }
        public bool FotoCapturada { get; set; }
        public bool Firmado { get; set; }
        public string? HashSha256 { get; set; }
    }

    public class CharlaDetalleDto
    {
        public int Id { get; set; }
        public string Tema { get; set; } = "";
        public string Instructor { get; set; } = "";
        public string Obra { get; set; } = "";
        public string Cuadrilla { get; set; } = "";
        public DateTime FechaCharla { get; set; }
        public int DuracionMinutos { get; set; }
        public int TotalAsistentes { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public double PrecisionGps { get; set; }
        public bool GpsCapturado { get; set; }
        public bool FotoCapturada { get; set; }
        public int ConteoFacial { get; set; }
        public bool Firmado { get; set; }
        public string? HoraFirma { get; set; }
        public string? HashSha256 { get; set; }
        public List<AsistenteDto> Asistentes { get; set; } = new();
    }

    public class AsistenteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string Cargo { get; set; } = "";
        public bool Presente { get; set; } = true;
    }

    public class CrearCharlaDto
    {
        public string Tema { get; set; } = "";
        public string Instructor { get; set; } = "";
        public int ObraId { get; set; }
        public string Obra { get; set; } = "";
        public string Cuadrilla { get; set; } = "";
        public DateTime FechaCharla { get; set; }
        public int DuracionMinutos { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public double PrecisionGps { get; set; }
        public string? FotoBase64 { get; set; }
        public string? FirmaBase64 { get; set; }
        public List<AsistenteDto> Asistentes { get; set; } = new();
    }
}
