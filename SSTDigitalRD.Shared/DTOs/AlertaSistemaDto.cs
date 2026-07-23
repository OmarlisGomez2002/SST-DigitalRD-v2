using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTDigitalRD.Shared.DTOs
{
    public class AlertaSistemaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Tipo { get; set; } = "ia";
        public string Nivel { get; set; } = "info";
        public string Zona { get; set; } = "";
        public int NivelRiesgo { get; set; }
        public bool Leida { get; set; }
        public string Fecha { get; set; } = "";
    }
}
