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
    public class CharlasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHashService _hash;
        private readonly IGeofencingService _geo;
        private readonly IPdfService _pdf;

        public CharlasController(AppDbContext db, IHashService hash, IGeofencingService geo, IPdfService pdf)
        {
            _db = db;
            _hash = hash;
            _geo = geo;
            _pdf = pdf;
        }

        // ── GET /api/charlas ───────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<List<CharlaListDto>>> GetTodas(
            [FromQuery] string? instructor = null,
            [FromQuery] string? cuadrilla = null)
        {
            var query = _db.Charlas.AsNoTracking();

            if (!string.IsNullOrEmpty(instructor))
                query = query.Where(x => x.Instructor.Contains(instructor));

            if (!string.IsNullOrEmpty(cuadrilla))
                query = query.Where(x => x.Cuadrilla == cuadrilla);

            var lista = await query
                .OrderByDescending(x => x.FechaCharla)
                .Select(x => new CharlaListDto
                {
                    Id = x.Id,
                    Tema = x.Tema,
                    Instructor = x.Instructor,
                    Obra = x.Obra,
                    Cuadrilla = x.Cuadrilla,
                    FechaCharla = x.FechaCharla,
                    DuracionMinutos = x.DuracionMinutos,
                    TotalAsistentes = x.TotalAsistentes,
                    AsistentesPresentes = x.AsistentesPresentes,
                    GpsCapturado = x.GpsCapturado,
                    FotoCapturada = x.FotoCapturada,
                    Firmado = x.Firmado,
                    HashSha256 = x.HashSha256
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ── GET /api/charlas/{id} ──────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CharlaDetalleDto>> GetPorId(int id)
        {
            var charla = await _db.Charlas
                .AsNoTracking()
                .Include(x => x.Asistentes)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (charla is null) return NotFound();

            return Ok(MapearDetalle(charla));
        }

        // ── POST /api/charlas ──────────────────────────────────
        [HttpPost]
        public async Task<ActionResult<CharlaDetalleDto>> Crear([FromBody] CrearCharlaDto dto)
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

            var charla = new Charla
            {
                Tema = dto.Tema,
                Instructor = dto.Instructor,
                Obra = dto.Obra,
                Cuadrilla = dto.Cuadrilla,
                FechaCharla = dto.FechaCharla,
                DuracionMinutos = dto.DuracionMinutos,
                TotalAsistentes = dto.Asistentes.Count,
                AsistentesPresentes = dto.Asistentes.Count(a => a.Presente),
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                PrecisionGps = dto.PrecisionGps,
                GpsCapturado = true,
                FotoBase64 = dto.FotoBase64,
                FotoCapturada = !string.IsNullOrEmpty(dto.FotoBase64),
                ConteoFacial = dto.Asistentes.Count(a => a.Presente),
                FirmaBase64 = dto.FirmaBase64,
                Firmado = !string.IsNullOrEmpty(dto.FirmaBase64),
                HoraFirma = DateTime.Now.ToString("hh:mm tt"),
                Asistentes = dto.Asistentes.Select(a => new AsistenteCharla
                {
                    Nombre = a.Nombre,
                    Cedula = a.Cedula,
                    Cargo = a.Cargo,
                    Presente = a.Presente
                }).ToList()
            };

            // Generar SHA-256
            charla.HashSha256 = _hash.GenerarHash(new
            {
                charla.Tema,
                charla.Instructor,
                charla.FechaCharla,
                charla.Latitud,
                charla.Longitud,
                charla.TotalAsistentes,
                charla.FirmaBase64
            });

            _db.Charlas.Add(charla);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPorId),
                new { id = charla.Id }, MapearDetalle(charla));
        }

        // ── PUT /api/charlas/{id} ──────────────────────────────
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id,
            [FromBody] CrearCharlaDto dto)
        {
            var charla = await _db.Charlas
                .Include(x => x.Asistentes)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (charla is null) return NotFound();

            charla.Tema = dto.Tema;
            charla.Cuadrilla = dto.Cuadrilla;
            charla.DuracionMinutos = dto.DuracionMinutos;
            charla.TotalAsistentes = dto.Asistentes.Count(a => a.Presente);
            charla.FechaActualizacion = DateTime.UtcNow;

            _db.AsistentesCharla.RemoveRange(charla.Asistentes);
            charla.Asistentes = dto.Asistentes.Select(a => new AsistenteCharla
            {
                Nombre = a.Nombre,
                Cedula = a.Cedula,
                Cargo = a.Cargo,
                Presente = a.Presente
            }).ToList();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── DELETE /api/charlas/{id} ───────────────────────────
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var charla = await _db.Charlas.FindAsync(id);
            if (charla is null) return NotFound();

            _db.Charlas.Remove(charla);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── GET /api/charlas/{id}/pdf ──────────────────────────
        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var charla = await _db.Charlas
                .AsNoTracking()
                .Include(x => x.Asistentes)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (charla is null) return NotFound();

            var dto = MapearDetalle(charla);
            var pdfBytes = _pdf.GenerarCharlaPdf(dto);

            return File(pdfBytes, "application/pdf",
                $"Charla_{charla.Tema.Replace(" ", "_")}_" +
                $"{charla.FechaCharla:yyyyMMdd}.pdf");
        }

        // ── GET /api/charlas/export ────────────────────────────
        [HttpGet("export")]
        public async Task<IActionResult> ExportarCsv(
            [FromQuery] string? cuadrilla = null,
            [FromQuery] string? instructor = null)
        {
            var query = _db.Charlas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(cuadrilla))
                query = query.Where(x => x.Cuadrilla == cuadrilla);

            if (!string.IsNullOrEmpty(instructor))
                query = query.Where(x => x.Instructor.Contains(instructor));

            var charlas = await query
                .OrderByDescending(x => x.FechaCharla)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Tema,Instructor,Obra,Cuadrilla,Fecha," +
                          "Duración (min),Asistentes,GPS,Firmado,Hash SHA-256");

            foreach (var c in charlas)
            {
                sb.AppendLine(string.Join(",",
                    EscaparCsv(c.Tema),
                    EscaparCsv(c.Instructor),
                    EscaparCsv(c.Obra),
                    EscaparCsv(c.Cuadrilla),
                    c.FechaCharla.ToString("dd/MM/yyyy"),
                    c.DuracionMinutos,
                    c.TotalAsistentes,
                    c.GpsCapturado ? "Sí" : "No",
                    c.Firmado ? "Sí" : "No",
                    c.HashSha256 ?? ""
                ));
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8
                    .GetBytes(sb.ToString()))
                .ToArray();

            return File(bytes, "text/csv",
                $"Charlas_{DateTime.Now:yyyyMMdd}.csv");
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
        private static CharlaDetalleDto MapearDetalle(Charla x) => new()
        {
            Id = x.Id,
            Tema = x.Tema,
            Instructor = x.Instructor,
            Obra = x.Obra,
            Cuadrilla = x.Cuadrilla,
            FechaCharla = x.FechaCharla,
            DuracionMinutos = x.DuracionMinutos,
            TotalAsistentes = x.TotalAsistentes,
            Latitud = x.Latitud,
            Longitud = x.Longitud,
            PrecisionGps = x.PrecisionGps,
            GpsCapturado = x.GpsCapturado,
            FotoCapturada = x.FotoCapturada,
            ConteoFacial = x.ConteoFacial,
            Firmado = x.Firmado,
            HoraFirma = x.HoraFirma,
            HashSha256 = x.HashSha256,
            Asistentes = x.Asistentes.Select(a => new AsistenteDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Cedula = a.Cedula,
                Cargo = a.Cargo,
                Presente = a.Presente
            }).ToList()
        };
    }
}
