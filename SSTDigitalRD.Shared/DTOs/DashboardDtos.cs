namespace SSTDigitalRD.Shared.DTOs;

public class DashboardMetricasDto
{
    public int CumplimientoNormativo { get; set; }
    public int IncidentesEsteMes { get; set; }
    public int UsoEPPDetectado { get; set; }
    public int CharlasMes { get; set; }    //public int InspeccionesPendientes { get; set; }
    public int TendenciaInspecciones { get; set; }
    public int TendenciaIncidentes { get; set; }
    public int TendenciaEPP { get; set; }
    public string ProximaEntregaMTRAB { get; set; } = "";
}

public class DashboardIncidentesSemanaDto
{
    public string Semana { get; set; } = "";
    public int Reportados { get; set; }
    public int Graves { get; set; }
}

public class DashboardAlertaDto
{
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Tiempo { get; set; } = "";
}

public class DashboardInspeccionRecienteDto
{
    public int Id { get; set; }
    public string Area { get; set; } = "";
    public string Inspector { get; set; } = "";
    public string Fecha { get; set; } = "";
    public string Estado { get; set; } = "";
}

public class DashboardResumenDto
{
    public DashboardMetricasDto Metricas { get; set; } = new();
    public List<DashboardIncidentesSemanaDto> IncidentesPorSemana { get; set; } = new();
    public List<DashboardAlertaDto> AlertasActivas { get; set; } = new();
    public List<DashboardInspeccionRecienteDto> UltimasInspecciones { get; set; } = new();
    public int PctCascos { get; set; }
    public int PctChalecos { get; set; }
    public int PctBotas { get; set; }
}

//public class AlertaDto
//{
//    public string Titulo { get; set; } = string.Empty;
//    public string Descripcion { get; set; } = string.Empty;
//    public string Tiempo { get; set; } = string.Empty;
//    public string Nivel { get; set; } = "warning";
//}

//public class InspeccionResumenDto
//{
//    public int Id { get; set; }
//    public string Area { get; set; } = string.Empty;
//    public string Inspector { get; set; } = string.Empty;
//    public DateTime Fecha { get; set; }
//    public string Estado { get; set; } = string.Empty;
//}

//public class EppDeteccionDto
//{
//    public string Nombre { get; set; } = string.Empty;
//    public int Porcentaje { get; set; }
//    public string Color { get; set; } = "#185FA5";
//}