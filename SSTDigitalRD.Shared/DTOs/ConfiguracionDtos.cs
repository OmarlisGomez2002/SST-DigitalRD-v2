using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTDigitalRD.Shared.DTOs
{
    public class ConfiguracionEmpresaDto
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = "";
        public string RNC { get; set; } = "";
        public string Sector { get; set; } = "";
        public string ResponsableSST { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Correo { get; set; } = "";
    }

    public class ObraActivaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Direccion { get; set; } = "";
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public int RadioGeofencing { get; set; } = 100;
        public bool Activa { get; set; } = true;
    }

    public class NotificacionConfigDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public bool Activa { get; set; } = true;
    }

    public class UsuarioSistemaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Rol { get; set; } = "Inspector";
        public string Cuadrilla { get; set; } = "";
        public bool Activo { get; set; } = true;
        // Para el avatar en el frontend
        public string Iniciales { get; set; } = "";
        public string AvatarBg { get; set; } = "#E6F1FB";
        public string AvatarColor { get; set; } = "#185FA5";
    }

    public class CrearUsuarioDto
    {
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Rol { get; set; } = "Inspector";
        public string Cuadrilla { get; set; } = "";
    }

    public class TipoInspeccionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public bool Activo { get; set; } = true;
    }

    public class CuadrillaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public bool Activa { get; set; } = true;
    }

    public class TipoCharlaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public bool Activo { get; set; } = true;
    }

    public class CargoEmpleadoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public bool Activo { get; set; } = true;
    }

    public class ItemChecklistDto
    {
        public int Id { get; set; }
        public string Categoria { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public bool Activo { get; set; } = true;
        public int Orden { get; set; }
    }

    public class TipoEPPDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Icono { get; set; } = "ti-hardhat";
        public bool Activo { get; set; } = true;
    }

    public class ProgramaSSTDto
    {
        public int Id { get; set; }
        public string Vigencia { get; set; } = "2025 — 2027";
        public string Politica { get; set; } = "";
        public string MatrizRiesgos { get; set; } = "";
        public string PlanEmergencia { get; set; } = "";
        public string FechaAprobacion { get; set; } = "";
    }

    public class ZonaObraDto
    {
        public int Id { get; set; }
        public int ObraId { get; set; }
        public string ObraNombre { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public bool Activa { get; set; } = true;
    }

}
