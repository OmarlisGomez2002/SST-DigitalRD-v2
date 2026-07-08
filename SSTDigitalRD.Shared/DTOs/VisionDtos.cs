using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTDigitalRD.Shared.DTOs
{
    public class CrearCapturaVisionDto
    {
        public string Area { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public bool TieneInfraccion { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public int PctCasco { get; set; }
        public int PctChaleco { get; set; }
        public int PctBotas { get; set; }
        public string? ImageBase64 { get; set; }
    }

    public class CapturaVisionListDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Area { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public bool TieneInfraccion { get; set; }
        public string HashSha256 { get; set; } = "";
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public int PctCasco { get; set; }
        public int PctChaleco { get; set; }
        public int PctBotas { get; set; }
    }
}
