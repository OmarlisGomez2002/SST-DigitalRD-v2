using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Server.Services;
using SSTDigitalRD.Shared.DTOs;

namespace SSTDigitalRD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DossierController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHashService _hash;
        private readonly IPdfService _pdf;

        public DossierController(AppDbContext db, IHashService hash, IPdfService pdf)
        {
            _db = db;
            _hash = hash;
            _pdf = pdf;
        }

        // ── GET /api/dossier ───────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<DossierResumenDto>> GetResumen([FromQuery] string? periodo = null)
        {
            var ahora = DateTime.UtcNow;
            
            // Parsear el período seleccionado si viene
            if (!string.IsNullOrEmpty(periodo) &&
                DateTime.TryParseExact(periodo,
                    new[] { "MMMM yyyy", "MMMM\u00a0yyyy" },
                    new System.Globalization.CultureInfo("es-DO"),
                    System.Globalization.DateTimeStyles.None,
                    out var fechaPeriodo))
            {
                ahora = new DateTime(
                    fechaPeriodo.Year,
                    fechaPeriodo.Month,
                    DateTime.DaysInMonth(fechaPeriodo.Year, fechaPeriodo.Month),
                    23, 59, 59);
            }

            var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);

            // ── Reporte A — Programa de SST ────────────────────
            var reporteA = new ReporteStatusDto
            {
                Letra = "A",
                Titulo = "Programa de SST",
                BaseLegal = "Art. 8, Reg. 522-06",
                Frecuencia = "Vigencia trienal",
                Periodo = "2025 — 2027",
                Estado = "generado",
                Completitud = 100,
                Detalle = "Actualizado: 01/01/2025",
                RegistrosIncluidos =
                    "Política SST, matriz de riesgos, plan de emergencias",
                HashSha256 = _hash.GenerarHash(new
                {
                    letra = "A",
                    periodo = "2025-2027",
                    ts = "2025-01-01"
                }),
                FechaGeneracion = "01/01/2025"
            };

            // ── Reporte B — Registro de accidentes ────────────
            var incidentesMes = await _db.Incidentes
                .Where(x => x.FechaIncidente >= inicioMes)
                .CountAsync();

            var reporteB = new ReporteStatusDto
            {
                Letra = "B",
                Titulo = "Registro de accidentes",
                BaseLegal = "Art. 6.1.3, Reg. 522-06",
                Frecuencia = "Generación inmediata",
                Periodo = ahora.ToString("MMMM yyyy"),
                Estado = incidentesMes > 0 ? "generado" : "pendiente",
                Completitud = incidentesMes > 0 ? 100 : 0,
                Detalle = $"{incidentesMes} incidente(s) este mes",
                RegistrosIncluidos =
                    $"{incidentesMes} incidentes con GPS, fotos y firma",
                HashSha256 = incidentesMes > 0
                    ? _hash.GenerarHash(new
                    {
                        letra = "B",
                        mes = ahora.Month,
                        total = incidentesMes
                    })
                    : "",
                FechaGeneracion = ahora.ToString("dd/MM/yyyy"),
                MensajePendiente = incidentesMes == 0
                    ? "No hay incidentes registrados este mes."
                    : ""
            };

            // ── Reporte C — Actas del Comité Mixto ────────────
            var charlasMes = await _db.Charlas
                .Where(x => x.FechaCharla >= inicioMes)
                .CountAsync();

            var reporteC = new ReporteStatusDto
            {
                Letra = "C",
                Titulo = "Actas del Comité Mixto",
                BaseLegal = "Res. 007-2011",
                Frecuencia = "Mensual",
                Periodo = ahora.ToString("MMMM yyyy"),
                Estado = charlasMes > 0 ? "generado" : "pendiente",
                Completitud = charlasMes > 0 ? 100 : 0,
                Detalle = $"Última reunión: {ahora:dd/MM/yyyy}",
                RegistrosIncluidos =
                    $"{charlasMes} acta(s) con asistencia firmada",
                HashSha256 = charlasMes > 0
                    ? _hash.GenerarHash(new
                    {
                        letra = "C",
                        mes = ahora.Month,
                        total = charlasMes
                    })
                    : "",
                FechaGeneracion = ahora.ToString("dd/MM/yyyy"),
                MensajePendiente = charlasMes == 0
                    ? "No hay actas del comité registradas este mes."
                    : ""
            };

            // ── Reporte D — Capacitaciones ─────────────────────
            var reporteD = new ReporteStatusDto
            {
                Letra = "D",
                Titulo = "Registro de capacitaciones",
                BaseLegal = "Art. 9.6, Reg. 522-06",
                Frecuencia = "Continuo",
                Periodo = ahora.ToString("MMMM yyyy"),
                Estado = charlasMes > 0 ? "generado" : "pendiente",
                Completitud = charlasMes > 0 ? 100 : 0,
                Detalle = $"{charlasMes} charla(s) este mes",
                RegistrosIncluidos =
                    $"{charlasMes} charlas con asistencia y firma",
                HashSha256 = charlasMes > 0
                    ? _hash.GenerarHash(new
                    {
                        letra = "D",
                        mes = ahora.Month,
                        total = charlasMes
                    })
                    : "",
                FechaGeneracion = ahora.ToString("dd/MM/yyyy"),
                MensajePendiente = charlasMes == 0
                    ? "No hay capacitaciones registradas este mes."
                    : ""
            };

            // ── Reporte E — Inventario EPP ─────────────────────
            var totalEntregas = await _db.EntregasEPP.CountAsync();
            var artVencidos = await _db.ArticulosEPP
                .CountAsync(x => x.Estado == "Vencido");

            var estadoE = artVencidos > 0 ? "pendiente" : "generado";
            var pctE = totalEntregas > 0
                ? Math.Max(0, 100 - artVencidos * 10)
                : 0;

            var reporteE = new ReporteStatusDto
            {
                Letra = "E",
                Titulo = "Inventario de EPP",
                BaseLegal = "Res. 04/2007",
                Frecuencia = "Continuo",
                Periodo = ahora.ToString("MMMM yyyy"),
                Estado = estadoE,
                Completitud = pctE,
                Detalle = artVencidos > 0
                    ? $"{artVencidos} artículo(s) vencido(s)"
                    : "Todo vigente",
                RegistrosIncluidos =
                    $"{totalEntregas} entregas registradas",
                HashSha256 = estadoE == "generado"
                    ? _hash.GenerarHash(new
                    {
                        letra = "E",
                        total = totalEntregas
                    })
                    : "",
                FechaGeneracion = ahora.ToString("dd/MM/yyyy"),
                MensajePendiente = artVencidos > 0
                    ? $"Hay {artVencidos} artículo(s) vencido(s) sin " +
                      "registro de reposición."
                    : ""
            };

            // ── Reporte F — Notificación accidente grave ───────
            var accGravesPendientes = await _db.Incidentes
                .CountAsync(x =>
                    x.Tipo == "Accidente grave" &&
                    !x.NotificadoMTRAB &&
                    x.FechaIncidente >= ahora.AddDays(-2));

            var reporteF = new ReporteStatusDto
            {
                Letra = "F",
                Titulo = "Notificación accidente grave",
                BaseLegal = "Art. 6, Código de Trabajo",
                Frecuencia = "24-48h tras el evento",
                Periodo = ahora.ToString("dd/MM/yyyy"),
                Estado = accGravesPendientes > 0 ? "urgente" : "generado",
                Completitud = accGravesPendientes > 0 ? 0 : 100,
                Detalle = accGravesPendientes > 0
                    ? $"{accGravesPendientes} accidente(s) grave(s) pendiente(s)"
                    : "Sin accidentes graves recientes",
                RegistrosIncluidos = accGravesPendientes > 0
                    ? $"{accGravesPendientes} incidente(s) requieren notificación"
                    : "Sin notificaciones pendientes",
                HashSha256 = accGravesPendientes == 0
                    ? _hash.GenerarHash(new
                    {
                        letra = "F",
                        fecha = ahora.Date
                    })
                    : "",
                FechaGeneracion = ahora.ToString("dd/MM/yyyy"),
                MensajePendiente = accGravesPendientes > 0
                    ? $"Hay {accGravesPendientes} accidente(s) grave(s) que " +
                      "requieren notificación al MTRAB en las próximas 48 horas."
                    : ""
            };

            var reportes = new List<ReporteStatusDto>
        {
            reporteA, reporteB, reporteC,
            reporteD, reporteE, reporteF
        };

            var generados = reportes.Count(r => r.Estado == "generado");
            var pctTotal = (int)Math.Round(
                reportes.Average(r => r.Completitud));

            return Ok(new DossierResumenDto
            {
                ReportesGenerados = generados,
                TotalReportes = reportes.Count,
                PorcentajeCompletitud = pctTotal,
                UltimaGeneracion = ahora.ToString("dd/MM · yyyy"),
                DiasProximaEntrega = 18,
                Reportes = reportes
            });
        }

        // ── POST /api/dossier/generar ──────────────────────────
        [HttpPost("generar")]
        public async Task<ActionResult<ReporteStatusDto>> GenerarReporte(
            [FromBody] GenerarReporteDto dto)
        {
            // Obtener el resumen actualizado y devolver el reporte específico
            var resumen = (await GetResumen()).Value;
            if (resumen is null) return StatusCode(500);

            var reporte = resumen.Reportes
                .FirstOrDefault(r => r.Letra == dto.Letra);

            if (reporte is null)
                return NotFound(new { error = $"Reporte {dto.Letra} no existe." });

            // Marcar accidentes graves como notificados si es reporte F
            if (dto.Letra == "F")
            {
                var accGraves = await _db.Incidentes
                    .Where(x =>
                        x.Tipo == "Accidente grave" &&
                        !x.NotificadoMTRAB &&
                        x.FechaIncidente >= DateTime.UtcNow.AddDays(-2))
                    .ToListAsync();

                foreach (var acc in accGraves)
                    acc.NotificadoMTRAB = true;

                await _db.SaveChangesAsync();
            }

            return Ok(reporte);
        }

        [HttpGet("{letra}/pdf")]
        public async Task<IActionResult> DescargarPDF(string letra)
        {
            try
            {
                // Reconstruir el resumen directamente sin llamar GetResumen()
                var ahora = DateTime.UtcNow;
                var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);

                var incidentesMes = await _db.Incidentes
                    .Where(x => x.FechaIncidente >= inicioMes)
                    .CountAsync();

                var charlasMes = await _db.Charlas
                    .Where(x => x.FechaCharla >= inicioMes)
                    .CountAsync();

                var totalEntregas = await _db.EntregasEPP.CountAsync();
                var artVencidos = await _db.ArticulosEPP
                    .CountAsync(x => x.Estado == "Vencido");

                var accGravesPendientes = await _db.Incidentes
                    .CountAsync(x =>
                        x.Tipo == "Accidente grave" &&
                        !x.NotificadoMTRAB &&
                        x.FechaIncidente >= ahora.AddDays(-2));

                // Construir el reporte específico
                ReporteStatusDto? reporte = letra.ToUpper() switch
                {
                    "A" => new ReporteStatusDto
                    {
                        Letra = "A",
                        Titulo = "Programa de SST",
                        BaseLegal = "Art. 8, Reg. 522-06",
                        Frecuencia = "Vigencia trienal",
                        Periodo = "2025 — 2027",
                        Estado = "generado",
                        Completitud = 100,
                        Detalle = "Actualizado: 01/01/2025",
                        RegistrosIncluidos =
                            "Política SST, matriz de riesgos, plan de emergencias",
                        HashSha256 = _hash.GenerarHash(new
                        {
                            letra = "A",
                            periodo = "2025-2027",
                            ts = "2025-01-01"
                        }),
                        FechaGeneracion = "01/01/2025"
                    },
                    "B" => new ReporteStatusDto
                    {
                        Letra = "B",
                        Titulo = "Registro de accidentes",
                        BaseLegal = "Art. 6.1.3, Reg. 522-06",
                        Frecuencia = "Generación inmediata",
                        Periodo = ahora.ToString("MMMM yyyy"),
                        Estado = incidentesMes > 0 ? "generado" : "pendiente",
                        Completitud = incidentesMes > 0 ? 100 : 0,
                        Detalle = $"{incidentesMes} incidente(s) este mes",
                        RegistrosIncluidos =
                            $"{incidentesMes} incidentes con GPS, fotos y firma",
                        HashSha256 = incidentesMes > 0
                            ? _hash.GenerarHash(new
                            {
                                letra = "B",
                                mes = ahora.Month,
                                total = incidentesMes
                            }) : "",
                        FechaGeneracion = ahora.ToString("dd/MM/yyyy")
                    },
                    "C" => new ReporteStatusDto
                    {
                        Letra = "C",
                        Titulo = "Actas del Comité Mixto",
                        BaseLegal = "Res. 007-2011",
                        Frecuencia = "Mensual",
                        Periodo = ahora.ToString("MMMM yyyy"),
                        Estado = charlasMes > 0 ? "generado" : "pendiente",
                        Completitud = charlasMes > 0 ? 100 : 0,
                        Detalle = $"Última reunión: {ahora:dd/MM/yyyy}",
                        RegistrosIncluidos =
                            $"{charlasMes} acta(s) con asistencia firmada",
                        HashSha256 = charlasMes > 0
                            ? _hash.GenerarHash(new
                            {
                                letra = "C",
                                mes = ahora.Month,
                                total = charlasMes
                            }) : "",
                        FechaGeneracion = ahora.ToString("dd/MM/yyyy")
                    },
                    "D" => new ReporteStatusDto
                    {
                        Letra = "D",
                        Titulo = "Registro de capacitaciones",
                        BaseLegal = "Art. 9.6, Reg. 522-06",
                        Frecuencia = "Continuo",
                        Periodo = ahora.ToString("MMMM yyyy"),
                        Estado = charlasMes > 0 ? "generado" : "pendiente",
                        Completitud = charlasMes > 0 ? 100 : 0,
                        Detalle = $"{charlasMes} charla(s) este mes",
                        RegistrosIncluidos =
                            $"{charlasMes} charlas con asistencia y firma",
                        HashSha256 = charlasMes > 0
                            ? _hash.GenerarHash(new
                            {
                                letra = "D",
                                mes = ahora.Month,
                                total = charlasMes
                            }) : "",
                        FechaGeneracion = ahora.ToString("dd/MM/yyyy")
                    },
                    "E" => new ReporteStatusDto
                    {
                        Letra = "E",
                        Titulo = "Inventario de EPP",
                        BaseLegal = "Res. 04/2007",
                        Frecuencia = "Continuo",
                        Periodo = ahora.ToString("MMMM yyyy"),
                        Estado = artVencidos > 0 ? "pendiente" : "generado",
                        Completitud = totalEntregas > 0
                            ? Math.Max(0, 100 - artVencidos * 10) : 0,
                        Detalle = artVencidos > 0
                            ? $"{artVencidos} artículo(s) vencido(s)"
                            : "Todo vigente",
                        RegistrosIncluidos =
                            $"{totalEntregas} entregas registradas",
                        HashSha256 = artVencidos == 0
                            ? _hash.GenerarHash(new
                            {
                                letra = "E",
                                total = totalEntregas
                            }) : "",
                        FechaGeneracion = ahora.ToString("dd/MM/yyyy")
                    },
                    "F" => new ReporteStatusDto
                    {
                        Letra = "F",
                        Titulo = "Notificación accidente grave",
                        BaseLegal = "Art. 6, Código de Trabajo",
                        Frecuencia = "24-48h tras el evento",
                        Periodo = ahora.ToString("dd/MM/yyyy"),
                        Estado = accGravesPendientes > 0
                            ? "urgente" : "generado",
                        Completitud = accGravesPendientes > 0 ? 0 : 100,
                        Detalle = accGravesPendientes > 0
                            ? $"{accGravesPendientes} accidente(s) grave(s) pendiente(s)"
                            : "Sin accidentes graves recientes",
                        RegistrosIncluidos = accGravesPendientes > 0
                            ? $"{accGravesPendientes} incidente(s) requieren notificación"
                            : "Sin notificaciones pendientes",
                        HashSha256 = accGravesPendientes == 0
                            ? _hash.GenerarHash(new
                            {
                                letra = "F",
                                fecha = ahora.Date
                            }) : "",
                        FechaGeneracion = ahora.ToString("dd/MM/yyyy")
                    },
                    _ => null
                };

                if (reporte is null)
                    return NotFound(new { error = $"Reporte {letra} no existe." });

                var empresa = await _db.ConfiguracionEmpresa
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                var nombreEmpresa = empresa?.RazonSocial
                    ?? "Empresa SST-Digital RD";

                var pdfBytes = _pdf.GenerarDossierPdf(reporte, nombreEmpresa);

                return File(pdfBytes, "application/pdf",
                    $"Dossier_{reporte.Letra}_" +
                    $"{reporte.Titulo.Replace(" ", "_")}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
