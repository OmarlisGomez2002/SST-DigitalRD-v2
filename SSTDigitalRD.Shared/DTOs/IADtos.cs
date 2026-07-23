namespace SSTDigitalRD.Shared.DTOs
{
    public class IAAnalisisDto
    {
        public int IndiceRiesgoGlobal { get; set; }
        public int PrecisionModelo { get; set; }
        public string HoraAnalisis { get; set; } = "";
        public int TotalEmpleados { get; set; }
        public List<ZonaRiesgoDto> Zonas { get; set; } = new();
        public List<string> TendenciaLabels { get; set; } = new(); // ← agregar
        public List<int> TendenciaDatos { get; set; } = new();
    }

    public class ZonaRiesgoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public int Trabajadores { get; set; }
        public int PorcentajeRiesgo { get; set; }
        public string Icono { get; set; } = "ti-alert-triangle";
        public string IconoBg { get; set; } = "#FAEEDA";
        public string IconoColor { get; set; } = "#854F0B";
        public List<FactorRiesgoDto> Factores { get; set; } = new();
        public List<AccionRecomendadaDto> Acciones { get; set; } = new();
    }

    //public class FactorRiesgoDto
    //{
    //    public string Descripcion { get; set; } = "";
    //    public bool Critico { get; set; }
    //    public FactorRiesgoDto(string desc, bool critico)
    //    { Descripcion = desc; Critico = critico; }
    //}

    public class FactorRiesgoDto
    {
        public string Descripcion { get; set; } = "";
        public bool Critico { get; set; }

        // Constructor sin parámetros para el deserializador
        public FactorRiesgoDto() { }

        // Constructor con parámetros para usarlo en el controlador
        public FactorRiesgoDto(string desc, bool critico)
        {
            Descripcion = desc;
            Critico = critico;
        }
    }

    //public class AccionRecomendadaDto
    //{
    //    public string Icono { get; set; } = "";
    //    public string Descripcion { get; set; } = "";
    //    public AccionRecomendadaDto(string icono, string desc)
    //    { Icono = icono; Descripcion = desc; }
    //}
    public class AccionRecomendadaDto
    {
        public string Icono { get; set; } = "";
        public string Descripcion { get; set; } = "";

        public AccionRecomendadaDto() { }

        public AccionRecomendadaDto(string icono, string desc)
        {
            Icono = icono;
            Descripcion = desc;
        }
    }

    public class AlertaIADto
    {
        public int ZonaId { get; set; }
        public string ZonaNombre { get; set; } = "";
        public int NivelRiesgo { get; set; }
    }
}