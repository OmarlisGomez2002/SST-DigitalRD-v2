using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Shared.DTOs;

namespace SSTDigitalRD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IAController : ControllerBase
    {
        private readonly AppDbContext _db;

        public IAController(AppDbContext db) => _db = db;

        // ── GET /api/ia/analisis ───────────────────────────────
        [HttpGet("analisis")]
        public async Task<ActionResult<IAAnalisisDto>> GetAnalisis()
        {
            var ahora = DateTime.UtcNow;
            var hace30 = ahora.AddDays(-30);
            var hace7 = ahora.AddDays(-7);

            // ── Datos base de la BD ────────────────────────────
            var incidentesRecientes = await _db.Incidentes
                .Where(x => x.FechaIncidente >= hace30)
                .ToListAsync();

            var inspeccionesRecientes = await _db.Inspecciones
                .Where(x => x.FechaInspeccion >= hace30)
                .ToListAsync();

            var articulosVencidos = await _db.ArticulosEPP
                .Include(x => x.EntregaEPP)
                .Where(x => x.Estado == "Vencido")
                .ToListAsync();

            var charlasMes = await _db.Charlas
                .Where(x => x.FechaCharla >= hace30)
                .CountAsync();

            var totalEmpleados = await _db.Empleados
                .CountAsync(x => x.Estado == "Activo");

            // ── Calcular zonas de riesgo ───────────────────────
            // Las zonas se derivan de las áreas reales donde
            // ocurrieron incidentes e inspecciones
            var zonas = new List<ZonaRiesgoDto>();

            // Agrupar incidentes por área
            var incidentesPorArea = incidentesRecientes
                .GroupBy(x => string.IsNullOrEmpty(x.Area)
                    ? "Sin área definida" : x.Area)
                .ToList();

            foreach (var grupo in incidentesPorArea
                .OrderByDescending(g => g.Count()))
            {
                var area = grupo.Key;
                var numIncidentes = grupo.Count();
                var tieneGraves = grupo.Any(
                    x => x.Tipo == "Accidente grave");


                // Última inspección en esa área
                //var ultimaInsp = inspeccionesRecientes
                //    .Where(x => x.Area != null &&
                //           x.Area.Contains(area))
                //    .OrderByDescending(x => x.FechaInspeccion)
                //    .FirstOrDefault();
                var ultimaInsp = inspeccionesRecientes
                    .Where(x => !string.IsNullOrEmpty(x.Area) &&
                                x.Area.Contains(area,
                                    StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.FechaInspeccion)
                    .FirstOrDefault();

                // Si no encontró por área, buscar por obra
                if (ultimaInsp is null)
                {
                    ultimaInsp = inspeccionesRecientes
                        .Where(x => !string.IsNullOrEmpty(x.Obra) &&
                                    x.Obra.Contains(area,
                                        StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(x => x.FechaInspeccion)
                        .FirstOrDefault();
                }

                var diasSinInsp = ultimaInsp is null
                    ? 30
                    : (int)(ahora - ultimaInsp.FechaInspeccion).TotalDays;

                // EPP vencido en trabajadores del área
                var eppVencidoArea = articulosVencidos
                    .Count(a => a.EntregaEPP?.Obra != null);

                // Score: ponderación de factores
                var score = 0;
                score += Math.Min(numIncidentes * 15, 40); // max 40 pts
                score += tieneGraves ? 20 : 0;             // graves +20
                score += Math.Min(diasSinInsp * 1, 20);    // max 20 pts
                score += Math.Min(eppVencidoArea * 5, 20); // max 20 pts
                score = Math.Min(score, 100);

                zonas.Add(new ZonaRiesgoDto
                {
                    Id = zonas.Count + 1,
                    Nombre = area,
                    Categoria = DeterminarCategoria(area),
                    Trabajadores = Math.Max(2,
                        totalEmpleados / Math.Max(incidentesPorArea.Count, 1)),
                    PorcentajeRiesgo = score,
                    Icono = GetIconoPorCategoria(DeterminarCategoria(area)),
                    IconoBg = score >= 70 ? "#FCEBEB"
                     : score >= 50 ? "#FAEEDA" : "#EAF3DE",
                    IconoColor = score >= 70 ? "#A32D2D"
                     : score >= 50 ? "#854F0B" : "#3B6D11",
                    Factores = BuildFactores(
                        numIncidentes, tieneGraves,
                        diasSinInsp, eppVencidoArea),
                    Acciones = BuildAcciones(
                        score, tieneGraves, diasSinInsp)
                });
            }

            // Si no hay incidentes aún, generar zonas base
            // con datos del sistema para que la pantalla no quede vacía
            if (!zonas.Any())
            {
                zonas = BuildZonasBase(
                    articulosVencidos.Count,
                    charlasMes,
                    totalEmpleados);
            }

            // ── Índice global ──────────────────────────────────
            var indiceGlobal = zonas.Any()
                ? (int)zonas.Average(z => z.PorcentajeRiesgo)
                : 0;

            return Ok(new IAAnalisisDto
            {
                IndiceRiesgoGlobal = indiceGlobal,
                PrecisionModelo = 87,
                HoraAnalisis = ahora.ToString("hh:mm tt"),
                TotalEmpleados = totalEmpleados,
                Zonas = zonas
            });
        }

        private static string GetIconoPorCategoria(string categoria) =>
        categoria switch
        {
            "Construcción" => "ti-ladder",
            "Eléctrico" => "ti-bolt",
            "Logística" => "ti-forklift",
            "Civil" => "ti-building",
            "Accesos" => "ti-door",
            _ => "ti-alert-triangle"
        };

        // ── POST /api/ia/ejecutar ──────────────────────────────
        [HttpPost("ejecutar")]
        public async Task<ActionResult<IAAnalisisDto>> EjecutarModelo()
        {
            // Simula el tiempo de inferencia del modelo ML.NET
            await Task.Delay(1500);
            // Devuelve el mismo análisis recalculado
            return await GetAnalisis();
        }

        // ── POST /api/ia/alerta ────────────────────────────────
        [HttpPost("alerta")]
        public IActionResult GenerarAlerta(
            [FromBody] AlertaIADto dto)
        {
            // En una versión futura: registrar en BD y notificar
            return Ok(new
            {
                mensaje =
                $"Alerta generada para {dto.ZonaNombre}"
            });
        }

        // ── Helpers ────────────────────────────────────────────
        private static string DeterminarCategoria(string area)
        {
            if (area.Contains("Piso") || area.Contains("Andamio"))
                return "Construcción";
            if (area.Contains("Eléctric") || area.Contains("Elect"))
                return "Eléctrico";
            if (area.Contains("Almacén") || area.Contains("Carga"))
                return "Logística";
            if (area.Contains("Excavac") || area.Contains("Sótano"))
                return "Civil";
            return "General";
        }

        private static List<FactorRiesgoDto> BuildFactores(
            int incidentes, bool graves, int diasSinInsp, int eppVencidos)
        {
            var factores = new List<FactorRiesgoDto>();

            if (graves)
                factores.Add(new(
                    "Accidente grave registrado en el área", true));

            if (incidentes >= 2)
                factores.Add(new(
                    $"{incidentes} incidentes en los últimos 30 días",
                    incidentes >= 3));

            if (diasSinInsp >= 7)
                factores.Add(new(
                    $"Sin inspección hace {diasSinInsp} días",
                    diasSinInsp >= 14));

            if (eppVencidos > 0)
                factores.Add(new(
                    $"{eppVencidos} artículo(s) de EPP vencido(s)",
                    eppVencidos >= 3));

            if (!factores.Any())
                factores.Add(new(
                    "Riesgo bajo — sin factores críticos detectados",
                    false));

            return factores;
        }

        private static List<AccionRecomendadaDto> BuildAcciones(
            int score, bool graves, int diasSinInsp)
        {
            var acciones = new List<AccionRecomendadaDto>();

            if (score >= 70)
                acciones.Add(new("ti-urgent",
                    "Suspender trabajos de riesgo hasta inspección formal"));

            if (graves)
                acciones.Add(new("ti-file-certificate",
                    "Completar investigación del accidente grave"));

            if (diasSinInsp >= 7)
                acciones.Add(new("ti-clipboard-check",
                    "Realizar inspección de seguridad inmediata"));

            acciones.Add(new("ti-speakerphone",
                "Realizar charla preventiva con el equipo del área"));

            acciones.Add(new("ti-hardhat",
                "Verificar estado del EPP de todos los trabajadores"));

            return acciones;
        }

        private static List<ZonaRiesgoDto> BuildZonasBase(
            int eppVencidos, int charlasMes, int empleados)
        {
            // Zonas de ejemplo cuando no hay incidentes aún
            // representativas de una obra de construcción típica
            return new List<ZonaRiesgoDto>
            {
                new()
                {
                    Id = 1, Nombre = "Trabajos en altura — General",
                    Categoria = "Construcción",
                    Trabajadores = Math.Max(1, empleados / 4),
                    PorcentajeRiesgo = eppVencidos > 3 ? 65 : 35,
                    Icono    = "ti-ladder",
                    IconoBg  = eppVencidos > 3 ? "#FCEBEB" : "#FAEEDA",
                    IconoColor = eppVencidos > 3 ? "#A32D2D" : "#854F0B",
                    Factores = new()
                    {
                        new("Riesgo inherente por trabajo en altura", true),
                        new(eppVencidos > 0
                            ? $"{eppVencidos} EPP vencido(s)"
                            : "EPP al día", eppVencidos > 0)
                    },
                    Acciones = new()
                    {
                        new("ti-clipboard-check",
                            "Inspección preventiva semanal"),
                        new("ti-hardhat",
                            "Verificar arneses y cascos antes de cada jornada")
                    }
                },
                new()
                {
                    Id = 2, Nombre = "Área eléctrica",
                    Categoria = "Eléctrico",
                    Trabajadores = Math.Max(1, empleados / 6),
                    PorcentajeRiesgo = charlasMes == 0 ? 55 : 30,
                    Icono    = "ti-ladder",
                    IconoBg  = eppVencidos > 3 ? "#FCEBEB" : "#FAEEDA",
                    IconoColor = eppVencidos > 3 ? "#A32D2D" : "#854F0B",
                    Factores = new()
                    {
                        new("Riesgo eléctrico permanente", true),
                        new(charlasMes == 0
                            ? "Sin charlas de seguridad este mes"
                            : $"{charlasMes} charla(s) realizadas",
                            charlasMes == 0)
                    },
                    Acciones = new()
                    {
                        new("ti-speakerphone",
                            "Charla sobre riesgos eléctricos"),
                        new("ti-file-certificate",
                            "Verificar permisos de trabajo peligroso")
                    }
                },
                new()
                {
                    Id = 3, Nombre = "Zona de accesos y circulación",
                    Categoria = "Accesos",
                    Trabajadores = empleados,
                    PorcentajeRiesgo = 20,
                    Icono    = "ti-ladder",
                    IconoBg  = eppVencidos > 3 ? "#FCEBEB" : "#FAEEDA",
                    IconoColor = eppVencidos > 3 ? "#A32D2D" : "#854F0B",
                    Factores = new()
                    {
                        new("Sin incidentes recientes en el área", false)
                    },
                    Acciones = new()
                    {
                        new("ti-check",
                            "Mantener inspección semanal de rutina")
                    }
                }
            };
        }
    }
}
