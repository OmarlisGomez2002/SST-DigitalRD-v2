using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTDigitalRD.Shared.DTOs
{
    public class EmpleadoListDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Cuadrilla { get; set; } = "";
        public string Obra { get; set; } = "";
        public string Estado { get; set; } = "";
    }

    public class EmpleadoDetalleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Cedula { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Cuadrilla { get; set; } = "";
        public string Obra { get; set; } = "";
        public DateTime FechaIngreso { get; set; }
        public string TipoContrato { get; set; } = "";
        public string NumeroTSS { get; set; } = "";
        public string Estado { get; set; } = "";
    }

    public class CrearEmpleadoDto
    {
        public string Nombre { get; set; } = "";
        public string Cedula { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Cuadrilla { get; set; } = "";
        public string Obra { get; set; } = "";
        public DateTime FechaIngreso { get; set; }
        public string TipoContrato { get; set; } = "Por obra";
        public string NumeroTSS { get; set; } = "";
        public string Estado { get; set; } = "Activo";
    }
}
