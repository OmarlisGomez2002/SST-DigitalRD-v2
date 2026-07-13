using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Server.Services;
using SSTDigitalRD.Shared.DTOs;
using System.Runtime.Intrinsics.X86;

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

            DateTime inicioMes;
            DateTime finMes;


            // Parsear el período seleccionado si viene
            if (!string.IsNullOrEmpty(periodo) &&
                DateTime.TryParseExact(periodo,
                    new[] { "MMMM yyyy", "MMMM\u00a0yyyy" },
                    new System.Globalization.CultureInfo("es-DO"),
                    System.Globalization.DateTimeStyles.None,
                    out var fechaPeriodo))
            {
                //ahora = new DateTime(
                //    fechaPeriodo.Year,
                //    fechaPeriodo.Month,
                //    DateTime.DaysInMonth(fechaPeriodo.Year, fechaPeriodo.Month),
                //    23, 59, 59);
                inicioMes = new DateTime(
        fechaPeriodo.Year, fechaPeriodo.Month, 1,
        0, 0, 0, DateTimeKind.Utc);
                finMes = inicioMes.AddMonths(1).AddSeconds(-1);
            }
            else
            {
                inicioMes = new DateTime(
                    ahora.Year, ahora.Month, 1,
                    0, 0, 0, DateTimeKind.Utc);
                finMes = inicioMes.AddMonths(1).AddSeconds(-1);
            }

            //var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);

            // ── Reporte A — Programa de SST ────────────────────
            var progSST = await _db.ProgramaSST
    .AsNoTracking()
    .FirstOrDefaultAsync();

            var registrosA = new List<string>();
            int camposCompletos = 0;

            //if (progSST is not null)
            //{
            //    if (!string.IsNullOrEmpty(progSST.Politica))
            //        registrosA.Add("Política SST");
            //    if (!string.IsNullOrEmpty(progSST.MatrizRiesgos))
            //        registrosA.Add("Matriz de riesgos");
            //    if (!string.IsNullOrEmpty(progSST.PlanEmergencia))
            //        registrosA.Add("Plan de emergencias");
            //}
            if (progSST is not null)
            {
                if (!string.IsNullOrEmpty(progSST.Politica))
                {
                    registrosA.Add("Política SST: " +
                        (progSST.Politica.Length > 60
                            ? progSST.Politica[..60] + "..."
                            : progSST.Politica));
                    camposCompletos++;
                }
                else registrosA.Add("Política SST: Pendiente de configurar");

                if (!string.IsNullOrEmpty(progSST.MatrizRiesgos))
                {
                    registrosA.Add("Matriz de riesgos: " +
                        (progSST.MatrizRiesgos.Length > 60
                            ? progSST.MatrizRiesgos[..60] + "..."
                            : progSST.MatrizRiesgos));
                    camposCompletos++;
                }
                else registrosA.Add("Matriz de riesgos: Pendiente de configurar");

                if (!string.IsNullOrEmpty(progSST.PlanEmergencia))
                {
                    registrosA.Add("Plan de emergencias: " +
                        (progSST.PlanEmergencia.Length > 60
                            ? progSST.PlanEmergencia[..60] + "..."
                            : progSST.PlanEmergencia));
                    camposCompletos++;
                }
                else registrosA.Add("Plan de emergencias: Pendiente de configurar");

                registrosA.Add($"Vigencia: {progSST.Vigencia}");
            }
            else
            {
                registrosA.Add("Programa SST no configurado");
                registrosA.Add("Acceda a Configuración → Programa SST para completar");
            }

            var completitudA = progSST is null ? 0
                : (camposCompletos * 100 / 3); //registrosA.Count

            // Estado: el programa SST es trienal, no depende del período
            var estadoA = completitudA == 100 ? "generado" : "pendiente";

            var reporteA = new ReporteStatusDto
            {
                Letra = "A",
                Titulo = "Programa de SST",
                BaseLegal = "Art. 8, Reg. 522-06",
                Frecuencia = "Vigencia trienal",
                Periodo = progSST?.Vigencia ?? "No configurado",
                Estado = estadoA, //completitudA == 100 ? "generado" : "pendiente",
                Completitud = completitudA,
                Detalle = progSST is not null
                    ? $"Actualizado: {progSST.FechaActualizacion:dd/MM/yyyy}" +
                        $"{camposCompletos}/3 secciones completas"
                    : "Sin configurar",
                RegistrosIncluidos = string.Join(" · ", registrosA), //registrosA.Any() ? string.Join(", ", registrosA) : "Programa no configurado",
                HashSha256 = estadoA == "generado" //completitudA == 100
                    ? _hash.GenerarHash(new
                    {
                        letra = "A",
                        vigencia = progSST!.Vigencia,
                        ts = progSST.FechaActualizacion
                    })
                    : "",
                FechaGeneracion = progSST?.FechaActualizacion
                    .ToString("dd/MM/yyyy") ?? "",
                MensajePendiente = estadoA == "pendiente" //completitudA < 100
                    ? $"El Programa SST está incompleto " +
          $"({camposCompletos}/3 secciones). Configure en " +
          "Configuración → Programa de SST."
        : ""
            };
            //var reporteA = new ReporteStatusDto
            //{
            //    Letra = "A",
            //    Titulo = "Programa de SST",
            //    BaseLegal = "Art. 8, Reg. 522-06",
            //    Frecuencia = "Vigencia trienal",
            //    Periodo = "2025 — 2027",
            //    Estado = "generado",
            //    Completitud = 100,
            //    Detalle = "Actualizado: 01/01/2025",
            //    RegistrosIncluidos =
            //        "Política SST, matriz de riesgos, plan de emergencias",
            //    HashSha256 = _hash.GenerarHash(new
            //    {
            //        letra = "A",
            //        periodo = "2025-2027",
            //        ts = "2025-01-01"
            //    }),
            //    FechaGeneracion = "01/01/2025"
            //};

            // ── Reporte B — Registro de accidentes ────────────
            var incidentesMes = await _db.Incidentes
                .Where(x => x.FechaIncidente >= inicioMes &&
                x.FechaIncidente <= finMes)
                .CountAsync();

            var reporteB = new ReporteStatusDto
            {
                Letra = "B",
                Titulo = "Registro de accidentes",
                BaseLegal = "Art. 6.1.3, Reg. 522-06",
                Frecuencia = "Generación inmediata",
                Periodo = inicioMes.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-DO")), //ahora.ToString("MMMM yyyy"),
                Estado = incidentesMes > 0 ? "generado" : "pendiente",
                Completitud = incidentesMes > 0 ? 100 : 0,
                Detalle = $"{incidentesMes} incidente(s) en {inicioMes:MMMM yyyy}",
                RegistrosIncluidos = $"{incidentesMes} incidentes con GPS, fotos y firma",
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
                .Where(x => x.FechaCharla >= inicioMes &&
                x.FechaCharla <= finMes)
                .CountAsync();

            var asistentesTotales = await _db.Charlas
    .Where(x => x.FechaCharla >= inicioMes &&
                x.FechaCharla <= finMes)
    .SumAsync(x => (int?)x.AsistentesPresentes) ?? 0;

            var reporteC = new ReporteStatusDto
            {
                Letra = "C",
                Titulo = "Actas del Comité Mixto",
                BaseLegal = "Res. 007-2011",
                Frecuencia = "Mensual",
                Periodo = ahora.ToString("MMMM yyyy"),
                Estado = charlasMes > 0 ? "generado" : "pendiente",
                Completitud = charlasMes > 0 ? 100 : 0,
                Detalle = $"{charlasMes} acta(s) en " + $"{inicioMes:MMMM yyyy}",
                RegistrosIncluidos = $"{charlasMes} acta(s) con asistencia firmada · " + $"{asistentesTotales} participantes",
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
                Detalle = $"{charlasMes} charla(s) en " + $"{inicioMes:MMMM yyyy}",
                RegistrosIncluidos = $"{charlasMes} charlas · " + $"{asistentesTotales} trabajadores capacitados",
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
            var entregasMes = await _db.EntregasEPP
    .Where(x => x.FechaEntrega >= inicioMes &&
                x.FechaEntrega <= finMes)
    .CountAsync();
            var totalEntregas = await _db.EntregasEPP.CountAsync();
            var artVencidos = await _db.ArticulosEPP
                .CountAsync(x => x.Estado == "Vencido");

            // Total solo cuenta las del período, no todas
            var estadoE = entregasMes == 0 ? "pendiente"
                          : artVencidos > 0 ? "pendiente"
                          : "generado";/*var estadoE = artVencidos > 0 ? "pendiente"
            : entregasMes == 0 ? "pendiente"
            : "generado";*/  //var estadoE = artVencidos > 0 ? "pendiente" : "generado";
            var pctE = entregasMes == 0 ? 0
              : artVencidos > 0 ? Math.Max(0, 100 - artVencidos * 10)
              : 100;

            /*var pctE = totalEntregas == 0 ? 0
         : artVencidos > 0 ? Math.Max(0, 100 - artVencidos * 10)
         : 100;*/  /*var pctE = totalEntregas > 0
                ? Math.Max(0, 100 - artVencidos * 10)
                : 0;*/

            var reporteE = new ReporteStatusDto
            {
                Letra = "E",
                Titulo = "Inventario de EPP",
                BaseLegal = "Res. 04/2007",
                Frecuencia = "Continuo",
                Periodo = ahora.ToString("MMMM yyyy"),
                Estado = estadoE,
                Completitud = pctE,
                Detalle = entregasMes == 0
        ? $"Sin entregas en {inicioMes:MMMM yyyy}"
        : artVencidos > 0
            ? $"{artVencidos} artículo(s) vencido(s)"
            : $"{entregasMes} entrega(s) al día",/*artVencidos > 0
        ? $"{artVencidos} artículo(s) vencido(s)"
        : $"{entregasMes} entrega(s) en " +
          $"{inicioMes:MMMM yyyy}",*/
                RegistrosIncluidos = entregasMes == 0
        ? $"Sin entregas en el período seleccionado"
        : $"{entregasMes} entrega(s) en el período" +
          (artVencidos > 0
              ? $" · {artVencidos} artículo(s) vencido(s)"
              : " · todo vigente"),
                /*$"{entregasMes} entrega(s) en el período · " +
                $"{totalEntregas} total registradas",*/
                HashSha256 = estadoE == "generado"
                    ? _hash.GenerarHash(new
                    {
                        letra = "E",
                        mes = inicioMes.Month,
                        anio = inicioMes.Year,
                        total = entregasMes
                    })
                    : "",
                FechaGeneracion = ahora.ToString("dd/MM/yyyy"),
                MensajePendiente = estadoE == "pendiente"
        ? entregasMes == 0
            ? $"No hay entregas de EPP registradas en " +
              $"{inicioMes:MMMM yyyy}."
            : $"Hay {artVencidos} artículo(s) vencido(s) " +
              "sin reposición."
        : ""/*artVencidos > 0
                    ? $"Hay {artVencidos} artículo(s) vencido(s) sin " +
                      "registro de reposición."
                    : ""*/
            };

            // ── Reporte F — Notificación accidente grave ───────
            var accGravesMes = await _db.Incidentes
    .CountAsync(x =>
        x.Tipo == "Accidente grave" &&
        x.FechaIncidente >= inicioMes &&
        x.FechaIncidente <= finMes);

            var accGravesPendientes = await _db.Incidentes
                .CountAsync(x =>
                    x.Tipo == "Accidente grave" &&
                    !x.NotificadoMTRAB &&
                    x.FechaIncidente >= inicioMes &&
        x.FechaIncidente <= finMes);
            // F: urgente si hay graves pendientes, generado si hay graves
            // notificados, pendiente si no hay accidentes graves en el período
            /*var estadoF = accGravesPendientes > 0 ? "urgente"
            : accGravesMes > 0 ? "generado"
            : "pendiente";*/
            //var estadoF = accGravesPendientes > 0 ? "urgente" : "generado"; // sin accidentes o todos notificados = generado
            //var pctF = accGravesPendientes > 0 ? 0 : 100;
            /*var pctF = accGravesPendientes > 0 ? 0
                        : accGravesMes > 0 ? 100 : 50;*/ // sin accidentes = cumplimiento parcial

            var estadoF = accGravesPendientes > 0 ? "urgente"
            : accGravesMes > 0 ? "generado"
            : "pendiente";

            var pctF = accGravesPendientes > 0 ? 0
                        : accGravesMes > 0 ? 100
                        : 0;

            var reporteF = new ReporteStatusDto
            {
                Letra = "F",
                Titulo = "Notificación accidente grave",
                BaseLegal = "Art. 6, Código de Trabajo",
                Frecuencia = "24-48h tras el evento",
                Periodo = ahora.ToString("dd/MM/yyyy"),
                Estado = estadoF,//accGravesPendientes > 0 ? "urgente" : "generado",
                Completitud = pctF, //accGravesPendientes > 0 ? 0 : 100,
                Detalle = accGravesMes == 0
        ? $"Sin accidentes graves en " +
          $"{inicioMes:MMMM yyyy}"
        : accGravesPendientes > 0
            ? $"{accGravesPendientes} pendiente(s) de " +
              $"{accGravesMes} en el período"
            : $"{accGravesMes} notificado(s) correctamente",/*accGravesMes > 0
        ? $"{accGravesPendientes} pendiente(s) de " +
          $"{accGravesMes} en el período"
        : $"Sin accidentes graves en " +
          $"{inicioMes:MMMM yyyy}",*/
                RegistrosIncluidos = accGravesMes == 0
        ? $"Sin accidentes graves en el período"
        : $"{accGravesMes} accidente(s) grave(s) · " +
          $"{accGravesPendientes} sin notificar · " +
          $"{accGravesMes - accGravesPendientes} notificado(s)",
                /*accGravesMes > 0
        ? $"{accGravesMes} accidente(s) grave(s) · " +
          $"{accGravesPendientes} sin notificar"
        : "Sin notificaciones pendientes",*/
                HashSha256 = accGravesPendientes == 0
                    ? _hash.GenerarHash(new
                    {
                        letra = "F",
                        mes = inicioMes.Month,
                        anio = inicioMes.Year,
                        total = accGravesMes
                        //fecha = ahora.Date
                    })
                    : "",
                FechaGeneracion = ahora.ToString("dd/MM/yyyy"),
                MensajePendiente = accGravesPendientes > 0
    ? $"Hay {accGravesPendientes} accidente(s) grave(s) " +
      "sin notificar al MTRAB en el período."
   : accGravesMes == 0
        ? $"No hubo accidentes graves en " +
          $"{inicioMes.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-DO"))}. " +
          "Este reporte no aplica para el período."
        : ""/*estadoF == "urgente"
        ? $"Hay {accGravesPendientes} accidente(s) grave(s) " +
          "sin notificar al MTRAB en el período."
        : estadoF == "pendiente"
            ? $"Sin accidentes graves en " +
              $"{inicioMes:MMMM yyyy}. " +
              "Estado normal."
        : ""*//*accGravesPendientes > 0
                    ? $"Hay {accGravesPendientes} accidente(s) grave(s) " +
                      "sin notificar al MTRAB en el período seleccionado."
                    : accGravesMes == 0
        ? $"No hay accidentes graves registrados en " +
          $"{inicioMes:MMMM yyyy}."
        : ""*/
            };

            var reportes = new List<ReporteStatusDto>
        {
            reporteA, reporteB, reporteC,
            reporteD, reporteE, reporteF
        };

            var generados = reportes.Count(r => r.Estado == "generado");
            var urgentes = reportes.Count(r => r.Estado == "urgente"); //
            var pctTotal = reportes.Any()
    ? (int)Math.Round(reportes.Average(r => r.Completitud))
    : 0;
            /*var pctTotal = (int)Math.Round(
                reportes.Average(r => r.Completitud));*/

            // Calcular días hasta próxima entrega (día 5 del mes siguiente) El día 5 de cada mes es la fecha límite de entrega al MTRAB según el Reg. 522-06
            var proximaEntrega = new DateTime(
                ahora.Year, ahora.Month, 1)
                .AddMonths(1)
                .AddDays(4); // día 5

            var diasProxima = (int)(proximaEntrega - ahora).TotalDays;

            return Ok(new DossierResumenDto
            {
                ReportesGenerados = generados,
                TotalReportes = reportes.Count,
                PorcentajeCompletitud = pctTotal,
                UltimaGeneracion = ahora.ToString("dd/MM · yyyy", new System.Globalization.CultureInfo("es-DO")),
                DiasProximaEntrega = Math.Max(0, diasProxima),
                Reportes = reportes
            });
        }

        // ── POST /api/dossier/generar ──────────────────────────
        //[HttpPost("generar")]
        //public async Task<ActionResult<ReporteStatusDto>> GenerarReporte(
        //    [FromBody] GenerarReporteDto dto)
        //{
        //    // Obtener el resumen actualizado y devolver el reporte específico
        //    var resumen = (await GetResumen()).Value;
        //    if (resumen is null) return StatusCode(500);

        //    var reporte = resumen.Reportes
        //        .FirstOrDefault(r => r.Letra == dto.Letra);

        //    if (reporte is null)
        //        return NotFound(new { error = $"Reporte {dto.Letra} no existe." });

        //    // Marcar accidentes graves como notificados si es reporte F
        //    if (dto.Letra == "F")
        //    {
        //        var accGraves = await _db.Incidentes
        //            .Where(x =>
        //                x.Tipo == "Accidente grave" &&
        //                !x.NotificadoMTRAB &&
        //                x.FechaIncidente >= DateTime.UtcNow.AddDays(-2))
        //            .ToListAsync();

        //        foreach (var acc in accGraves)
        //            acc.NotificadoMTRAB = true;

        //        await _db.SaveChangesAsync();
        //    }

        //    return Ok(reporte);
        //}
        [HttpPost("generar")]
        public async Task<ActionResult<ReporteStatusDto>> GenerarReporte(
    [FromBody] GenerarReporteDto dto)
        {
            // Marcar accidentes graves como notificados si es reporte F
            if (dto.Letra == "F")
            {
                // Parsear período si viene
                DateTime inicioMes;
                DateTime finMes;

                if (!string.IsNullOrEmpty(dto.Periodo) &&
                    DateTime.TryParseExact(dto.Periodo,
                        new[] { "MMMM yyyy", "MMMM\u00a0yyyy" },
                        new System.Globalization.CultureInfo("es-DO"),
                        System.Globalization.DateTimeStyles.None,
                        out var fechaPeriodo))
                {
                    inicioMes = new DateTime(
                        fechaPeriodo.Year, fechaPeriodo.Month, 1,
                        0, 0, 0, DateTimeKind.Utc);
                    finMes = inicioMes.AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    var ahora = DateTime.UtcNow;
                    inicioMes = new DateTime(
                        ahora.Year, ahora.Month, 1,
                        0, 0, 0, DateTimeKind.Utc);
                    finMes = inicioMes.AddMonths(1).AddSeconds(-1);
                }

                var accGraves = await _db.Incidentes
                    .Where(x =>
                        x.Tipo == "Accidente grave" &&
                        !x.NotificadoMTRAB &&
                        x.FechaIncidente >= inicioMes &&
                        x.FechaIncidente <= finMes)
                    .ToListAsync();

                foreach (var acc in accGraves)
                    acc.NotificadoMTRAB = true;

                await _db.SaveChangesAsync();
            }

            // Recargar el resumen con el período para devolver estado actualizado
            var resumen = (await GetResumen(dto.Periodo)).Value;
            if (resumen is null) return StatusCode(500);

            var reporte = resumen.Reportes
                .FirstOrDefault(r => r.Letra == dto.Letra);

            if (reporte is null)
                return NotFound(new { error = $"Reporte {dto.Letra} no existe." });

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

                var progSST = await _db.ProgramaSST
    .AsNoTracking()
    .FirstOrDefaultAsync();

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
                        RegistrosIncluidos = progSST is not null
                            ? string.Join(" · ", new[]
                            {
                                !string.IsNullOrEmpty(progSST.Politica)
                                    ? "Política SST configurada"
                                    : "Política SST pendiente",
                                !string.IsNullOrEmpty(progSST.MatrizRiesgos)
                                    ? "Matriz de riesgos configurada"
                                    : "Matriz de riesgos pendiente",
                                !string.IsNullOrEmpty(progSST.PlanEmergencia)
                                    ? "Plan de emergencias configurado"
                                    : "Plan de emergencias pendiente",
                                $"Vigencia: {progSST.Vigencia}"
                            })
                            : "Programa SST no configurado",//"Política SST, matriz de riesgos, plan de emergencias",
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
