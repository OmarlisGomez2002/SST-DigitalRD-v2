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

            //var inspPendientes = await _db.Inspecciones
            //    .CountAsync(x => x.Estado == "En proceso");

            var charlasMes = await _db.Charlas
    .Where(x => x.FechaCharla >= inicioMes)
    .CountAsync();

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

            //// ── EPP ────────────────────────────────────────────
            //var totalEPP = await _db.ArticulosEPP.CountAsync();
            //var vigentesEPP = await _db.ArticulosEPP
            //    .CountAsync(x => x.Estado == "Vigente");

            //var pctEPP = totalEPP > 0
            //    ? (int)Math.Round((double)vigentesEPP / totalEPP * 100)
            //    : 88;
            // ── EPP ────────────────────────────────────────────────────
            var totalTrabajadores = await _db.Empleados
                .CountAsync(x => x.Estado == "Activo");

            // Trabajadores que tienen al menos una entrega de EPP vigente
            var trabajadoresConEPP = await _db.EntregasEPP
                .Where(x => x.Articulos.Any(a => a.Estado == "Vigente"))
                .Select(x => x.CedulaTrabajador)
                .Distinct()
                .CountAsync();

            var pctEPP = totalTrabajadores > 0
                ? (int)Math.Round(
                    (double)trabajadoresConEPP / totalTrabajadores * 100)
                : 0;

            // Para el panel de EPP — por tipo
            var totalEPPArticulos = await _db.ArticulosEPP.CountAsync();
            var vigentesEPP = await _db.ArticulosEPP
                .CountAsync(x => x.Estado == "Vigente");
            var pctGeneral = totalEPPArticulos > 0
                ? (int)Math.Round(
                    (double)vigentesEPP / totalEPPArticulos * 100)
                : 0;

            // ── Alertas activas — desde tabla AlertasSistema + reglas ──
            var alertas = new List<DashboardAlertaDto>();

            // Alertas de IA guardadas (no leídas, últimas 3)
            var alertasIA = await _db.AlertasSistema
                .Where(x => !x.Leida)
                .OrderByDescending(x => x.FechaCreacion)
                .Take(3)
                .ToListAsync();

            foreach (var ia in alertasIA)
            {
                alertas.Add(new DashboardAlertaDto
                {
                    Titulo = ia.Titulo,
                    Descripcion = ia.Descripcion,
                    Tipo = ia.Nivel,
                    Tiempo = TiempoRelativo(ia.FechaCreacion, ahora)
                });
            }

            var eppVencidos = await _db.ArticulosEPP
                .Include(x => x.EntregaEPP)
                .Where(x => x.Estado == "Vencido")
                .Take(2)
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
            
            // Sin charla hoy
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
                    CharlasMes = charlasMes, //InspeccionesPendientes = inspPendientes,
                    TendenciaInspecciones = tendInsp,
                    TendenciaIncidentes = tendInc,
                    TendenciaEPP = 0,
                    ProximaEntregaMTRAB = "15 días"
                },
                IncidentesPorSemana = porSemana,
                AlertasActivas = alertas,
                UltimasInspecciones = ultimasInsp,
                PctCascos = pctGeneral, //pctEPP,
                PctChalecos = pctGeneral, //Math.Max(pctEPP - 7, 70),
                PctBotas = pctGeneral, //Math.Max(pctEPP - 10, 65)
            };

            return Ok(resumen);
        }

        private static string TiempoRelativo(DateTime fecha, DateTime ahora)
        {
            var diff = ahora - fecha;
            if (diff.TotalMinutes < 1) return "Ahora mismo";
            if (diff.TotalMinutes < 60) return $"Hace {(int)diff.TotalMinutes} min";
            if (diff.TotalHours < 24) return $"Hace {(int)diff.TotalHours} h";
            if (diff.TotalDays < 7) return $"Hace {(int)diff.TotalDays} días";
            return fecha.ToString("dd/MM/yyyy");
        }
    }
}
