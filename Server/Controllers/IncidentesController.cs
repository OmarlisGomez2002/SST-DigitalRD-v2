using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Server.Models;
using SSTDigitalRD.Server.Services;
using SSTDigitalRD.Shared.DTOs;

namespace SSTDigitalRD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHashService _hash;
        private readonly IGeofencingService _geo;
        private readonly IPdfService _pdf;

        public IncidentesController(AppDbContext db, IHashService hash, IGeofencingService geo, IPdfService pdf)
        {
            _db = db;
            _hash = hash;
            _geo = geo;
            _pdf = pdf;
        }

        // ── GET /api/incidentes ────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<List<IncidenteListDto>>> GetTodos(
            [FromQuery] string? tipo = null,
            [FromQuery] string? estado = null,
            [FromQuery] int? obra = null)
        {
            var query = _db.Incidentes.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(x => x.Tipo == tipo);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(x => x.Estado == estado);

            if (obra.HasValue && obra.Value > 0)          
                query = query.Where(x => x.ObraId == obra.Value);

            var lista = await query
                .OrderByDescending(x => x.FechaIncidente)
                .Select(x => new IncidenteListDto
                {
                    Id = x.Id,
                    Descripcion = x.Descripcion,
                    Tipo = x.Tipo,
                    Area = x.Area,
                    Obra = x.Obra,
                    ObraId = x.ObraId,
                    Afectado = x.Afectado,
                    Inspector = x.Inspector,
                    Fecha = x.FechaIncidente,
                    DiasPerdidos = x.DiasPerdidos,
                    GpsCapturado = x.GpsCapturado,
                    Firmado = x.Firmado,
                    Estado = x.Estado,
                    HashSha256 = x.HashSha256
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ── GET /api/incidentes/{id} ───────────────────────────
        [HttpGet("{id:int}")]
        public async Task<ActionResult<IncidenteDetalleDto>> GetPorId(int id)
        {
            var inc = await _db.Incidentes
                .AsNoTracking()
                .Include(x => x.AccionesCorrectivas)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (inc is null) return NotFound();
            return Ok(MapearDetalle(inc));
        }

        // ── POST /api/incidentes ───────────────────────────────
        [HttpPost]
        public async Task<ActionResult<IncidenteDetalleDto>> Crear(
            [FromBody] CrearIncidenteDto dto)
        {
            // Validar geofencing
            //const double obraLat = 18.473621;
            //const double obraLng = -69.931215;

            // Buscar obra activa — primero por nombre, luego la primera activa
            //var obra = await _db.ObrasActivas.Where(x => x.Activa).OrderByDescending(x => x.Id).FirstOrDefaultAsync();

            // Buscar la obra seleccionada por el usuario
            ObraActiva? obra = null;

            if (dto.ObraId > 0)
            {
                obra = await _db.ObrasActivas.FirstOrDefaultAsync(x => x.Id == dto.ObraId && x.Activa);
            }

            // Si no encontró por ID buscar la primera activa
            obra ??= await _db.ObrasActivas.Where(x => x.Activa).OrderByDescending(x => x.Id).FirstOrDefaultAsync();

            // Validar geofencing solo si hay obra con radio configurado
            if (obra is not null && obra.RadioGeofencing > 0)
            {
                if (!_geo.EstaEnPerimetro(dto.Latitud, dto.Longitud, obra.Latitud, obra.Longitud, obra.RadioGeofencing))
                {
                    return BadRequest(new
                    {
                        error = $"Fuera del perímetro de '{obra.Nombre}'. " + $"Debe estar dentro de " + $"{obra.RadioGeofencing} metros para registrar."
                    });
                }
            }

            var inc = new Incidente
            {
                Descripcion = dto.Descripcion,
                Tipo = dto.Tipo,
                Area = dto.Area,
                ObraId = dto.ObraId,
                Obra = dto.Obra,
                Afectado = dto.Afectado,
                Inspector = dto.Inspector,
                AtencionMedica = dto.AtencionMedica,
                Testigos = dto.Testigos,
                FechaIncidente = dto.Fecha,
                DiasPerdidos = dto.DiasPerdidos,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                PrecisionGps = dto.PrecisionGps,
                GpsCapturado = true,
                CantidadFotos = dto.CantidadFotos,
                NotificarMTRAB = dto.NotificarMTRAB,
                FirmaBase64 = dto.FirmaBase64,
                Firmado = !string.IsNullOrEmpty(dto.FirmaBase64),
                Estado = "En seguimiento",
                AccionesCorrectivas = dto.AccionesCorrectivas
                    .Select(a => new AccionCorrectiva
                    {
                        Descripcion = a.Descripcion,
                        Responsable = a.Responsable,
                        FechaLimite = a.FechaLimite,
                        Estado = "Pendiente"
                    }).ToList()
            };

            // Generar SHA-256
            inc.HashSha256 = _hash.GenerarHash(new
            {
                inc.Descripcion,
                inc.Tipo,
                inc.Area,
                inc.Inspector,
                inc.FechaIncidente,
                inc.Latitud,
                inc.Longitud,
                inc.FirmaBase64
            });

            _db.Incidentes.Add(inc);
            await _db.SaveChangesAsync();

            // Si requiere notificación al MTRAB, marcar automáticamente
            if (inc.NotificarMTRAB)
            {
                // TODO: Integrar envío de notificación via email/API MTRAB
                inc.NotificadoMTRAB = false;
                await _db.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetPorId),
                new { id = inc.Id }, MapearDetalle(inc));
        }

        // ── PUT /api/incidentes/{id} ───────────────────────────
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id,
            [FromBody] CrearIncidenteDto dto)
        {
            var inc = await _db.Incidentes
                .Include(x => x.AccionesCorrectivas)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (inc is null) return NotFound();

            inc.Descripcion = dto.Descripcion;
            inc.Area = dto.Area;
            inc.Afectado = dto.Afectado;
            inc.AtencionMedica = dto.AtencionMedica;
            inc.Testigos = dto.Testigos;
            inc.DiasPerdidos = dto.DiasPerdidos;
            inc.FechaActualizacion = DateTime.UtcNow;

            _db.AccionesCorrectivas.RemoveRange(inc.AccionesCorrectivas);
            inc.AccionesCorrectivas = dto.AccionesCorrectivas
                .Select(a => new AccionCorrectiva
                {
                    Descripcion = a.Descripcion,
                    Responsable = a.Responsable,
                    FechaLimite = a.FechaLimite,
                    Estado = a.Estado
                }).ToList();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── PUT /api/incidentes/{id}/cerrar ───────────────────
        [HttpPut("{id:int}/cerrar")]
        public async Task<IActionResult> Cerrar(int id)
        {
            var inc = await _db.Incidentes.FindAsync(id);
            if (inc is null) return NotFound();

            inc.Estado = "Cerrado";
            inc.FechaActualizacion = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── DELETE /api/incidentes/{id} ────────────────────────
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var inc = await _db.Incidentes.FindAsync(id);
            if (inc is null) return NotFound();

            _db.Incidentes.Remove(inc);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── GET /api/incidentes/{id}/pdf ───────────────────────
        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var inc = await _db.Incidentes
                .AsNoTracking()
                .Include(x => x.AccionesCorrectivas)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (inc is null) return NotFound();

            var dto = MapearDetalle(inc);
            var pdfBytes = _pdf.GenerarIncidentePdf(dto);

            return File(pdfBytes, "application/pdf",
                $"Incidente_{inc.Tipo.Replace(" ", "_")}_" +
                $"{inc.FechaIncidente:yyyyMMdd}.pdf");
        }

        // ── GET /api/incidentes/export ─────────────────────────
        [HttpGet("export")]
        public async Task<IActionResult> ExportarCsv(
            [FromQuery] string? tipo = null,
            [FromQuery] string? estado = null)
        {
            var query = _db.Incidentes.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(x => x.Tipo == tipo);
            if (!string.IsNullOrEmpty(estado))
                query = query.Where(x => x.Estado == estado);

            var lista = await query
                .OrderByDescending(x => x.FechaIncidente)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Descripción,Tipo,Área,Obra,Afectado," +
                          "Inspector,Fecha,Días perdidos," +
                          "GPS,Firmado,Estado,Hash SHA-256");

            foreach (var i in lista)
            {
                sb.AppendLine(string.Join(",",
                    EscaparCsv(i.Descripcion),
                    EscaparCsv(i.Tipo),
                    EscaparCsv(i.Area),
                    EscaparCsv(i.Obra),
                    EscaparCsv(i.Afectado ?? ""),
                    EscaparCsv(i.Inspector),
                    i.FechaIncidente.ToString("dd/MM/yyyy"),
                    i.DiasPerdidos,
                    i.GpsCapturado ? "Sí" : "No",
                    i.Firmado ? "Sí" : "No",
                    EscaparCsv(i.Estado),
                    i.HashSha256 ?? ""
                ));
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();

            return File(bytes, "text/csv",
                $"Incidentes_{DateTime.Now:yyyyMMdd}.csv");
        }

        private static string EscaparCsv(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return "";
            if (valor.Contains(',') || valor.Contains('"') ||
                valor.Contains('\n'))
                return $"\"{valor.Replace("\"", "\"\"")}\"";
            return valor;
        }

        // ── Helper ─────────────────────────────────────────────
        private static IncidenteDetalleDto MapearDetalle(Incidente x) => new()
        {
            Id = x.Id,
            Descripcion = x.Descripcion,
            Tipo = x.Tipo,
            Area = x.Area,
            Obra = x.Obra,
            Afectado = x.Afectado,
            Inspector = x.Inspector,
            AtencionMedica = x.AtencionMedica,
            Testigos = x.Testigos,
            Fecha = x.FechaIncidente,
            DiasPerdidos = x.DiasPerdidos,
            Latitud = x.Latitud,
            Longitud = x.Longitud,
            PrecisionGps = x.PrecisionGps,
            GpsCapturado = x.GpsCapturado,
            CantidadFotos = x.CantidadFotos,
            NotificarMTRAB = x.NotificarMTRAB,
            NotificadoMTRAB = x.NotificadoMTRAB,
            Estado = x.Estado,
            Firmado = x.Firmado,
            HashSha256 = x.HashSha256,
            AccionesCorrectivas = x.AccionesCorrectivas
                .Select(a => new AccionCorrectivaDto
                {
                    Id = a.Id,
                    Descripcion = a.Descripcion,
                    Responsable = a.Responsable,
                    FechaLimite = a.FechaLimite,
                    Estado = a.Estado
                }).ToList()
        };
    }
}
