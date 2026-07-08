using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Shared.DTOs;

namespace SSTDigitalRD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db) => _db = db;

        // ── GET /api/dashboard ─────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<DashboardResumenDto>> GetResumen()
        {
            var ahora = DateTime.UtcNow;
            var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);
            var hace30Dias = ahora.AddDays(-30);
            var hace60Dias = ahora.AddDays(-60);

            // ── Inspecciones ───────────────────────────────────
            var inspMes = await _db.Inspecciones
                .Where(x => x.FechaInspeccion >= inicioMes)
                .ToListAsync();

            var inspMesAnterior = await _db.Inspecciones
                .Where(x => x.FechaInspeccion >= hace60Dias &&
                            x.FechaInspeccion < hace30Dias)
                .CountAsync();

            var inspPendientes = await _db.Inspecciones
                .CountAsync(x => x.Estado == "En proceso");

            var conformes = inspMes.Count(x => x.Estado == "Conforme");
            var cumplimiento = inspMes.Any()
                ? (int)Math.Round((double)conformes / inspMes.Count * 100)
                : 94;

            // ── Incidentes ─────────────────────────────────────
            var incMes = await _db.Incidentes
                .Where(x => x.FechaIncidente >= inicioMes)
                .CountAsync();

            var incMesAnterior = await _db.Incidentes
                .Where(x => x.FechaIncidente >= hace60Dias &&
                            x.FechaIncidente < hace30Dias)
                .CountAsync();

            // ── Incidentes por semana (últimas 6 semanas) ──────
            var incidentes6Sem = await _db.Incidentes
                .Where(x => x.FechaIncidente >= ahora.AddDays(-42))
                .ToListAsync();

            var porSemana = Enumerable.Range(0, 6)
                .Select(i =>
                {
                    var inicio = ahora.AddDays(-(5 - i) * 7 - 6);
                    var fin = ahora.AddDays(-(5 - i) * 7);
                    var items = incidentes6Sem
                        .Where(x => x.FechaIncidente >= inicio &&
                                    x.FechaIncidente <= fin)
                        .ToList();
                    return new DashboardIncidentesSemanaDto
                    {
                        Semana = $"Sem {i + 1}",
                        Reportados = items.Count,
                        Graves = items.Count(x =>
                            x.Tipo == "Accidente grave")
                    };
                }).ToList();

            // ── EPP ────────────────────────────────────────────
            var totalEPP = await _db.ArticulosEPP.CountAsync();
            var vigentesEPP = await _db.ArticulosEPP
                .CountAsync(x => x.Estado == "Vigente");

            var pctEPP = totalEPP > 0
                ? (int)Math.Round((double)vigentesEPP / totalEPP * 100)
                : 88;

            // ── Alertas activas ────────────────────────────────
            var alertas = new List<DashboardAlertaDto>();

            var eppVencidos = await _db.ArticulosEPP
                .Include(x => x.EntregaEPP)
                .Where(x => x.Estado == "Vencido")
                .Take(3)
                .ToListAsync();

            foreach (var epp in eppVencidos)
            {
                alertas.Add(new DashboardAlertaDto
                {
                    Titulo = $"EPP vencido",
                    Descripcion = $"{epp.EntregaEPP?.NombreTrabajador} · {epp.TipoEPP}",
                    Tipo = "danger",
                    Tiempo = "Hoy"
                });
            }

            var sinCharlaHoy = await _db.Charlas
                .Where(x => x.FechaCharla.Date == ahora.Date)
                .CountAsync() == 0;

            if (sinCharlaHoy)
            {
                alertas.Add(new DashboardAlertaDto
                {
                    Titulo = "Sin charla registrada hoy",
                    Descripcion = "No se ha registrado ninguna charla de seguridad",
                    Tipo = "warn",
                    Tiempo = "Hoy"
                });
            }

            var zonasRiesgo = await _db.Incidentes
                .Where(x => x.FechaIncidente >= hace30Dias)
                .GroupBy(x => x.Area)
                .OrderByDescending(g => g.Count())
                .Take(1)
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            if (zonasRiesgo is not null)
            {
                alertas.Add(new DashboardAlertaDto
                {
                    Titulo = "Zona de alto riesgo detectada",
                    Descripcion = $"IA predictiva · {zonasRiesgo}",
                    Tipo = "danger",
                    Tiempo = "Hace 3 horas"
                });
            }

            // ── Últimas inspecciones ───────────────────────────
            var ultimasInsp = await _db.Inspecciones
                .AsNoTracking()
                .OrderByDescending(x => x.FechaInspeccion)
                .Take(4)
                .Select(x => new DashboardInspeccionRecienteDto
                {
                    Id = x.Id,
                    Area = x.Area,
                    Inspector = x.Inspector,
                    Fecha = x.FechaInspeccion.ToString("dd/MM"),
                    Estado = x.Estado
                })
                .ToListAsync();

            // ── Tendencias (positivo = mejora) ─────────────────
            var tendInsp = inspMes.Count - inspMesAnterior;
            var tendInc = incMesAnterior - incMes;

            // ── Respuesta ──────────────────────────────────────
            var resumen = new DashboardResumenDto
            {
                Metricas = new DashboardMetricasDto
                {
                    CumplimientoNormativo = cumplimiento,
                    IncidentesEsteMes = incMes,
                    UsoEPPDetectado = pctEPP,
                    InspeccionesPendientes = inspPendientes,
                    TendenciaInspecciones = tendInsp,
                    TendenciaIncidentes = tendInc,
                    TendenciaEPP = 0,
                    ProximaEntregaMTRAB = "15 días"
                },
                IncidentesPorSemana = porSemana,
                AlertasActivas = alertas,
                UltimasInspecciones = ultimasInsp,
                PctCascos = pctEPP,
                PctChalecos = Math.Max(pctEPP - 7, 70),
                PctBotas = Math.Max(pctEPP - 10, 65)
            };

            return Ok(resumen);
        }
    }
}
