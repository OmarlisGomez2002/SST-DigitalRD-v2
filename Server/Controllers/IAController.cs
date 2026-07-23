using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Server.Models;
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
        public async Task<ActionResult<IAAnalisisDto>> GetAnalisis([FromQuery] int? obraId = null, [FromQuery] string? periodo = null)
        {
            var ahora = DateTime.UtcNow;
            //var hace30 = ahora.AddDays(-30);
            //var hace7 = ahora.AddDays(-7);

            // ── Datos base de la BD ────────────────────────────
            // Parsear período si viene
            if (!string.IsNullOrEmpty(periodo) &&
                DateTime.TryParseExact(periodo,
                    new[] { "MMMM yyyy", "MMMM\u00a0yyyy" },
                    new System.Globalization.CultureInfo("es-DO"),
                    System.Globalization.DateTimeStyles.None,
                    out var fechaPeriodo))
            {
                ahora = new DateTime(
                    fechaPeriodo.Year, fechaPeriodo.Month,
                    DateTime.DaysInMonth(fechaPeriodo.Year, fechaPeriodo.Month),
                    23, 59, 59, DateTimeKind.Utc);
            }

            var inicioMes = new DateTime(ahora.Year, ahora.Month, 1,
                0, 0, 0, DateTimeKind.Utc);
            var hace30 = ahora.AddDays(-30);
            var hace7 = ahora.AddDays(-7);

            // Filtrar por obra si viene
            var queryInc = _db.Incidentes.AsQueryable();
            var queryInsp = _db.Inspecciones.AsQueryable();

            if (obraId.HasValue && obraId.Value > 0)
            {
                queryInc = queryInc.Where(x => x.ObraId == obraId.Value);
                queryInsp = queryInsp.Where(x => x.ObraId == obraId.Value);
            }

            var incidentesRecientes = await queryInc
        .Where(x => x.FechaIncidente >= hace30 &&
                    x.FechaIncidente <= ahora)
        .ToListAsync();

            var inspeccionesRecientes = await queryInsp
                .Where(x => x.FechaInspeccion >= hace30 &&
                            x.FechaInspeccion <= ahora)
                .ToListAsync();
            //var incidentesRecientes = await _db.Incidentes
            //    .Where(x => x.FechaIncidente >= hace30)
            //    .ToListAsync();

            //var inspeccionesRecientes = await _db.Inspecciones
            //    .Where(x => x.FechaInspeccion >= hace30)
            //    .ToListAsync();

            var articulosVencidos = await _db.ArticulosEPP
                .Include(x => x.EntregaEPP)
                .Where(x => x.Estado == "Vencido")
                .ToListAsync();

            var charlasMes = await _db.Charlas
                .Where(x => x.FechaCharla >= hace30)
                .CountAsync();

            var totalEmpleados = await _db.Empleados
                .CountAsync(x => x.Estado == "Activo");

            // ── Calcular zonas de riesgo ───────────────────────────────
            var zonas = new List<ZonaRiesgoDto>();

            // Cargar zonas configuradas para la obra seleccionada
            var zonasConfig = new List<SSTDigitalRD.Server.Models.ZonaObra>();
            if (obraId.HasValue && obraId.Value > 0)
            {
                zonasConfig = await _db.ZonasObra
                    .AsNoTracking()
                    .Where(x => x.ObraId == obraId.Value && x.Activa)
                    .OrderBy(x => x.Nombre)
                    .ToListAsync();
            }

            // Agrupar incidentes por área
            var incidentesPorArea = incidentesRecientes
                .GroupBy(x => string.IsNullOrEmpty(x.Area)
                    ? "Sin área definida" : x.Area)
                .ToDictionary(g => g.Key, g => g.ToList());

            if (zonasConfig.Any())
            {
                // Modo principal — zonas desde catálogo de la BD
                foreach (var zona in zonasConfig)
                {
                    var incsZona = incidentesPorArea.ContainsKey(zona.Nombre)
                        ? incidentesPorArea[zona.Nombre]
                        : new();

                    var numIncidentes = incsZona.Count;
                    var tieneGraves = incsZona.Any(x => x.Tipo == "Accidente grave");

                    var ultimaInsp = inspeccionesRecientes
                        .Where(x => x.Area == zona.Nombre)
                        .OrderByDescending(x => x.FechaInspeccion)
                        .FirstOrDefault();

                    var diasSinInsp = ultimaInsp is null
                        ? 30
                        : (int)(ahora - ultimaInsp.FechaInspeccion).TotalDays;

                    var eppVencidoArea = articulosVencidos
                        .Count(a => a.EntregaEPP?.Obra != null);

                    var score = 0;
                    if (numIncidentes > 0)
                    {
                        score += Math.Min(numIncidentes * 15, 40);
                        score += tieneGraves ? 20 : 0;
                        score += Math.Min(diasSinInsp, 20);
                        score += Math.Min(eppVencidoArea * 5, 20);
                        score = Math.Min(score, 100);
                    }

                    zonas.Add(new ZonaRiesgoDto
                    {
                        Id = zona.Id,
                        Nombre = zona.Nombre,
                        Categoria = DeterminarCategoria(zona.Nombre),
                        Trabajadores = Math.Max(2,
                            totalEmpleados / Math.Max(zonasConfig.Count, 1)),
                        PorcentajeRiesgo = score,
                        Icono = GetIconoPorCategoria(
                            DeterminarCategoria(zona.Nombre)),
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
            }
            else
            {
                // Fallback — sin zonas configuradas, agrupar por Area de incidentes
                foreach (var grupo in incidentesPorArea
                    .OrderByDescending(g => g.Value.Count))
                {
                    var area = grupo.Key;
                    var incs = grupo.Value;
                    var numIncidentes = incs.Count;
                    var tieneGraves = incs.Any(x => x.Tipo == "Accidente grave");

                    var ultimaInsp = inspeccionesRecientes
                        .Where(x => !string.IsNullOrEmpty(x.Area) &&
                                    x.Area.Contains(area,
                                        StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(x => x.FechaInspeccion)
                        .FirstOrDefault();

                    if (ultimaInsp is null)
                        ultimaInsp = inspeccionesRecientes
                            .Where(x => !string.IsNullOrEmpty(x.Obra) &&
                                        x.Obra.Contains(area,
                                            StringComparison.OrdinalIgnoreCase))
                            .OrderByDescending(x => x.FechaInspeccion)
                            .FirstOrDefault();

                    var diasSinInsp = ultimaInsp is null
                        ? 30
                        : (int)(ahora - ultimaInsp.FechaInspeccion).TotalDays;

                    var eppVencidoArea = articulosVencidos
                        .Count(a => a.EntregaEPP?.Obra != null);

                    var score = 0;
                    score += Math.Min(numIncidentes * 15, 40);
                    score += tieneGraves ? 20 : 0;
                    score += Math.Min(diasSinInsp, 20);
                    score += Math.Min(eppVencidoArea * 5, 20);
                    score = Math.Min(score, 100);

                    zonas.Add(new ZonaRiesgoDto
                    {
                        Id = zonas.Count + 1,
                        Nombre = area,
                        Categoria = DeterminarCategoria(area),
                        Trabajadores = Math.Max(2,
                            totalEmpleados / Math.Max(incidentesPorArea.Count, 1)),
                        PorcentajeRiesgo = score,
                        Icono = GetIconoPorCategoria(
                            DeterminarCategoria(area)),
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
            }

            // ── Calcular zonas de riesgo ───────────────────────
            // Las zonas se derivan de las áreas reales donde
            // ocurrieron incidentes e inspecciones
            /*var zonas = new List<ZonaRiesgoDto>();

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
            }*/

            // Si no hay incidentes aún, generar zonas base
            // con datos del sistema para que la pantalla no quede vacía
            //if (!zonas.Any())
            //{
            //    zonas = BuildZonasBase(
            //        articulosVencidos.Count,
            //        charlasMes,
            //        totalEmpleados);
            //}

            // Precisión estimada: % de incidentes con GPS + firma
            // sobre el total — indica calidad del dato de campo
            var totalInc = incidentesRecientes.Count;
            var incCompletos = incidentesRecientes
                .Count(x => x.GpsCapturado && x.Firmado);

            var precisionModelo = totalInc > 0
                ? (int)Math.Round((double)incCompletos / totalInc * 100)
                : 87;

            // ── Índice global ──────────────────────────────────
            var indiceGlobal = zonas.Any()
                ? (int)zonas.Average(z => z.PorcentajeRiesgo)
                : 0;

            var (tendLabels, tendDatos) = BuildTendenciaSemanal(
    incidentesRecientes, ahora);

            return Ok(new IAAnalisisDto
            {
                IndiceRiesgoGlobal = indiceGlobal,
                PrecisionModelo = precisionModelo, //87,
                HoraAnalisis = ahora.ToString("hh:mm tt"),
                TotalEmpleados = totalEmpleados,
                Zonas = zonas,
                TendenciaLabels = tendLabels,
                TendenciaDatos = tendDatos
            });
        }
        //cálculo semanal
        private static (List<string> labels, List<int> datos)
    BuildTendenciaSemanal(
        IEnumerable<SSTDigitalRD.Server.Models.Incidente> incidentes,
        DateTime ahora)
        {
            var labels = new List<string>();
            var datos = new List<int>();

            for (int i = 5; i >= 0; i--)
            {
                var inicioSemana = ahora.AddDays(-(i * 7 + 6))
                    .Date;
                var finSemana = ahora.AddDays(-(i * 7))
                    .Date.AddDays(1).AddSeconds(-1);

                var count = incidentes
                    .Count(x => x.FechaIncidente >= inicioSemana &&
                                x.FechaIncidente <= finSemana);

                labels.Add(i == 0 ? "Hoy"
                    : $"Sem -{i}");
                datos.Add(count);
            }

            return (labels, datos);
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
        public async Task<ActionResult<IAAnalisisDto>> EjecutarModelo([FromQuery] int? obraId = null, [FromQuery] string? periodo = null)
        {
            // Simula el tiempo de inferencia del modelo ML.NET
            await Task.Delay(1500);
            // Devuelve el mismo análisis recalculado
            return await GetAnalisis(obraId, periodo);
        }

        // ── POST /api/ia/alerta ────────────────────────────────
        [HttpPost("alerta")]
        public async Task<IActionResult> GenerarAlerta(
    [FromBody] AlertaIADto dto)
        {
            var alerta = new AlertaSistema
            {
                Titulo = $"Zona de riesgo crítico: {dto.ZonaNombre}",
                Descripcion = $"El modelo IA detectó un nivel de riesgo de " +
                              $"{dto.NivelRiesgo}% en la zona {dto.ZonaNombre}. " +
                              "Se recomienda inspección inmediata.",
                Tipo = "ia",
                Nivel = dto.NivelRiesgo >= 70 ? "danger"
                            : dto.NivelRiesgo >= 50 ? "warn" : "info",
                Zona = dto.ZonaNombre,
                NivelRiesgo = dto.NivelRiesgo,
                Leida = false,
                FechaCreacion = DateTime.UtcNow
            };

            _db.AlertasSistema.Add(alerta);
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = $"Alerta generada para {dto.ZonaNombre}" });
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
