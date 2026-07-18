using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Client.Pages;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Server.Models;
using SSTDigitalRD.Server.Services;
using SSTDigitalRD.Shared.DTOs;

namespace SSTDigitalRD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspeccionesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHashService _hash;
        private readonly IGeofencingService _geo;        
        private readonly IPdfService _pdf;

        public InspeccionesController(AppDbContext db, IHashService hash, IGeofencingService geo, IPdfService pdf)
        {
            _db = db;
            _hash = hash;
            _geo = geo;
            _pdf = pdf;
        }

        // ── GET /api/inspecciones ──────────────────────────────
        [HttpGet]
        public async Task<ActionResult<List<InspeccionListDto>>> GetTodas(
            [FromQuery] string? estado = null,
            [FromQuery] string? inspector = null,
    [FromQuery] int? obraId = null)
        {
            var query = _db.Inspecciones.AsNoTracking();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(x => x.Estado == estado);

            if (!string.IsNullOrEmpty(inspector))
                query = query.Where(x => x.Inspector
                    .Contains(inspector));

            if (obraId.HasValue && obraId.Value > 0)   
                query = query.Where(x => x.ObraId == obraId.Value);

            var lista = await query
                .OrderByDescending(x => x.FechaInspeccion)
                .Select(x => new InspeccionListDto
                {
                    Id = x.Id,
                    Area = x.Area,
                    Obra = x.Obra,
                    Inspector = x.Inspector,
                    Fecha = x.FechaInspeccion,
                    Estado = x.Estado,
                    GpsCapturado = x.GpsCapturado,
                    Firmado = x.Firmado
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ── GET /api/inspecciones/{id} ─────────────────────────
        [HttpGet("{id:int}")]
        public async Task<ActionResult<InspeccionDetalleDto>> GetPorId(int id)
        {
            var insp = await _db.Inspecciones
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (insp is null) return NotFound();

            return Ok(MapearDetalle(insp));
        }

        // ── POST /api/inspecciones ─────────────────────────────
        [HttpPost]
        public async Task<ActionResult<InspeccionDetalleDto>> Crear([FromBody] CrearInspeccionDto dto)
        {
            // Validar geofencing (coordenadas de ejemplo de Torre Piantini IV)
            //const double obraLat = 18.473621;  DESCOMENTAR 
            //const double obraLng = -69.931215;

            // Buscar obra activa — primero por nombre, luego la primera activa
            //var obra = await _db.ObrasActivas.Where(x => x.Activa).OrderByDescending(x => x.Id).FirstOrDefaultAsync();

            // Buscar la obra seleccionada por el usuario
            ObraActiva? obra = null;

            if (dto.ObraId > 0)
            {
                obra = await _db.ObrasActivas
                    .FirstOrDefaultAsync(x => x.Id == dto.ObraId && x.Activa);
            }

            // Si no encontró por ID buscar la primera activa
            obra ??= await _db.ObrasActivas
                .Where(x => x.Activa)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();


            // Validar geofencing solo si hay obra con radio configurado
            if (obra is not null && obra.RadioGeofencing > 0)
            {
                if (!_geo.EstaEnPerimetro(
                    dto.Latitud, dto.Longitud,
                    obra.Latitud, obra.Longitud,
                    obra.RadioGeofencing))
                {
                    return BadRequest(new
                    {
                        error = $"Fuera del perímetro de '{obra.Nombre}'. " +
                                $"Debe estar dentro de " +
                                $"{obra.RadioGeofencing} metros para registrar."
                    });
                }
            }

            var insp = new Inspeccion
            {
                Area = dto.Area,
                Obra = obra?.Nombre ?? dto.Obra,                //Obra = dto.Obra,
                TipoInspeccion = dto.TipoInspeccion,
                Inspector = dto.Inspector,
                ResponsableArea = dto.ResponsableArea,
                FechaInspeccion = dto.FechaInspeccion,
                CantidadTrabajadores = dto.CantidadTrabajadores,
                Descripcion = dto.Descripcion,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                PrecisionGps = dto.PrecisionGps,
                GpsCapturado = true,
                FirmaBase64 = dto.FirmaBase64,
                Firmado = !string.IsNullOrEmpty(dto.FirmaBase64),
                HoraFirma = DateTime.Now.ToString("hh:mm tt"),
                PlanAccion = dto.PlanAccion,
                Estado = DeterminarEstado(dto.Items),
                Items = dto.Items.Select(i => new ChecklistItem
                {
                    Categoria = i.Categoria,
                    Descripcion = i.Descripcion,
                    Resultado = i.Resultado,
                    Observacion = i.Observacion
                }).ToList()
            };

            // Generar SHA-256
            insp.HashSha256 = _hash.GenerarHash(new
            {
                insp.Area,
                insp.Inspector,
                insp.FechaInspeccion,
                insp.Latitud,
                insp.Longitud,
                insp.FirmaBase64
            });

            _db.Inspecciones.Add(insp);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPorId),
                new { id = insp.Id }, MapearDetalle(insp));
        }

        // ── PUT /api/inspecciones/{id} ─────────────────────────
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id,
            [FromBody] CrearInspeccionDto dto)
        {
            var insp = await _db.Inspecciones
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (insp is null) return NotFound();

            insp.ObraId = dto.ObraId;
            insp.Area = dto.Area;
            insp.Obra = dto.Obra;
            insp.TipoInspeccion = dto.TipoInspeccion;
            insp.Inspector = dto.Inspector;
            insp.ResponsableArea = dto.ResponsableArea;
            insp.FechaInspeccion = dto.FechaInspeccion;
            insp.CantidadTrabajadores = dto.CantidadTrabajadores;
            insp.Descripcion = dto.Descripcion;
            insp.PlanAccion = dto.PlanAccion;
            insp.Latitud = dto.Latitud;
            insp.Longitud = dto.Longitud;
            insp.PrecisionGps = dto.PrecisionGps;
            insp.PlanAccion = dto.PlanAccion ?? "";
            insp.Estado = DeterminarEstado(dto.Items);
            insp.FechaActualizacion = DateTime.UtcNow;

            _db.ChecklistItems.RemoveRange(insp.Items);
            insp.Items = dto.Items.Select(i => new ChecklistItem
            {
                Categoria = i.Categoria,
                Descripcion = i.Descripcion,
                Resultado = i.Resultado,
                Observacion = i.Observacion
            }).ToList();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── DELETE /api/inspecciones/{id} ─────────────────────
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var insp = await _db.Inspecciones.FindAsync(id);
            if (insp is null) return NotFound();

            _db.Inspecciones.Remove(insp);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── GET /api/inspecciones/{id}/pdf ─────────────────────
        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var insp = await _db.Inspecciones
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (insp is null) return NotFound();

            var dto = MapearDetalle(insp);
            var pdfBytes = _pdf.GenerarInspeccionPdf(dto);

            return File(pdfBytes, "application/pdf",
                $"Inspeccion_{insp.Area.Replace(" ", "_")}_{insp.FechaInspeccion:yyyyMMdd}.pdf");
        }

        // ── GET /api/inspecciones/export ───────────────────────
        [HttpGet("export")]
        public async Task<IActionResult> ExportarCsv(
            [FromQuery] string? estado = null,
            [FromQuery] string? inspector = null)
        {
            var query = _db.Inspecciones.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(x => x.Estado == estado);

            if (!string.IsNullOrEmpty(inspector))
                query = query.Where(x => x.Inspector.Contains(inspector));

            var inspecciones = await query
                .OrderByDescending(x => x.FechaInspeccion)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Área,Obra,Inspector,Fecha,Hora,GPS,Estado,Firmado,Trabajadores,Hash SHA-256");

            foreach (var i in inspecciones)
            {
                sb.AppendLine(string.Join(",",
                    EscaparCsv(i.Area),
                    EscaparCsv(i.Obra),
                    EscaparCsv(i.Inspector),
                    i.FechaInspeccion.ToString("dd/MM/yyyy"),
                    i.FechaInspeccion.ToString("hh:mm tt"),
                    i.GpsCapturado ? "Sí" : "No",
                    EscaparCsv(i.Estado),
                    i.Firmado ? "Sí" : "No",
                    i.CantidadTrabajadores,
                    i.HashSha256 ?? ""
                ));
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();

            return File(bytes, "text/csv",
                $"Inspecciones_{DateTime.Now:yyyyMMdd}.csv");
        }

        private static string EscaparCsv(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return "";
            if (valor.Contains(',') || valor.Contains('"') || valor.Contains('\n'))
                return $"\"{valor.Replace("\"", "\"\"")}\"";
            return valor;
        }

        // ── Helpers ────────────────────────────────────────────
        private static string DeterminarEstado(List<ChecklistItemDto> items)
        {            
            if (items.Any(x => x.Resultado == "no")) return "No conforme";
            if (items.Any(x => x.Resultado == "warn")) return "Observación";
            return "Conforme";
        }

        private static InspeccionDetalleDto MapearDetalle(Inspeccion x) => new()
        {
            Id = x.Id,
            Area = x.Area,
            Obra = x.Obra,
            ObraId = x.ObraId,
            TipoInspeccion = x.TipoInspeccion,
            Inspector = x.Inspector,
            ResponsableArea = x.ResponsableArea,
            FechaInspeccion = x.FechaInspeccion,
            CantidadTrabajadores = x.CantidadTrabajadores,
            Descripcion = x.Descripcion,
            Latitud = x.Latitud,
            Longitud = x.Longitud,
            PrecisionGps = x.PrecisionGps,
            GpsCapturado = x.GpsCapturado,
            Estado = x.Estado,
            Firmado = x.Firmado,
            HoraFirma = x.HoraFirma,
            HashSha256 = x.HashSha256,
            CantidadFotos = x.CantidadFotos,
            PlanAccion = x.PlanAccion,
            Items = x.Items.Select(i => new ChecklistItemDto
            {
                Id = i.Id,
                Categoria = i.Categoria,
                Descripcion = i.Descripcion,
                Resultado = i.Resultado,
                Observacion = i.Observacion
            }).ToList()
        };
    }
}
