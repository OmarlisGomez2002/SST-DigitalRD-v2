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
    public class EPPController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHashService _hash;
        private readonly IPdfService _pdf;
        private readonly IGeofencingService _geo;

        public EPPController(AppDbContext db, IHashService hash, IPdfService pdf, IGeofencingService geo)
        {
            _db = db;
            _hash = hash;
            _pdf = pdf;
            _geo = geo;
        }

        // ── GET /api/epp ───────────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<List<EntregaEPPListDto>>> GetTodas(
            [FromQuery] string? cuadrilla = null,
            [FromQuery] string? estado = null)
        {
            var query = _db.EntregasEPP
                .AsNoTracking()
                .Include(x => x.Articulos)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cuadrilla))
                query = query.Where(x => x.Cuadrilla == cuadrilla);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(x =>
                    x.Articulos.Any(a => a.Estado == estado));

            var lista = await query
                .OrderByDescending(x => x.FechaEntrega)
                .ToListAsync();

            return Ok(lista.Select(x => new EntregaEPPListDto
            {
                Id = x.Id,
                NombreTrabajador = x.NombreTrabajador,
                CedulaTrabajador = x.CedulaTrabajador,
                Cargo = x.Cargo,
                Cuadrilla = x.Cuadrilla,
                Obra = x.Obra,
                FechaEntrega = x.FechaEntrega,
                Firmado = x.Firmado,
                HashSha256 = x.HashSha256,
                EstadoGeneral = DeterminarEstadoGeneral(x.Articulos),
                Articulos = x.Articulos.Select(MapArticulo).ToList()
            }).ToList());
        }

        // ── GET /api/epp/{id} ──────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EntregaEPPDetalleDto>> GetPorId(int id)
        {
            var entrega = await _db.EntregasEPP
                .AsNoTracking()
                .Include(x => x.Articulos)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entrega is null) return NotFound();
            return Ok(MapearDetalle(entrega));
        }

        // ── GET /api/epp/trabajador/{cedula} ───────────────────
        [HttpGet("trabajador/{cedula}")]
        public async Task<ActionResult<EntregaEPPDetalleDto>> GetPorCedula(
            string cedula)
        {
            var entrega = await _db.EntregasEPP
                .AsNoTracking()
                .Include(x => x.Articulos)
                .Where(x => x.CedulaTrabajador == cedula)
                .OrderByDescending(x => x.FechaEntrega)
                .FirstOrDefaultAsync();

            if (entrega is null) return NotFound();
            return Ok(MapearDetalle(entrega));
        }

        // ── POST /api/epp ──────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult<EntregaEPPDetalleDto>> Crear([FromBody] CrearEntregaEPPDto dto)
        {
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

            var entrega = new EntregaEPP
            {
                NombreTrabajador = dto.NombreTrabajador,
                CedulaTrabajador = dto.CedulaTrabajador,
                Cargo = dto.Cargo,
                Cuadrilla = dto.Cuadrilla,
                Obra = dto.Obra,
                EntregadoPor = dto.EntregadoPor,
                FechaEntrega = dto.FechaEntrega,
                FirmaBase64 = dto.FirmaBase64,
                Firmado = !string.IsNullOrEmpty(dto.FirmaBase64),
                Articulos = dto.Articulos.Select(a => new ArticuloEPP
                {
                    TipoEPP = a.TipoEPP,
                    Categoria = a.Categoria,
                    Marca = a.Marca,
                    FechaVencimiento = a.FechaVencimiento,
                    Estado = a.Estado
                }).ToList()
            };

            entrega.HashSha256 = _hash.GenerarHash(new
            {
                entrega.NombreTrabajador,
                entrega.CedulaTrabajador,
                entrega.FechaEntrega,
                entrega.EntregadoPor,
                entrega.FirmaBase64,
                articulos = entrega.Articulos.Select(a => new
                {
                    a.TipoEPP,
                    a.Marca,
                    a.FechaVencimiento
                })
            });

            _db.EntregasEPP.Add(entrega);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPorId),
                new { id = entrega.Id }, MapearDetalle(entrega));
        }

        // ── PUT /api/epp/{id} ──────────────────────────────────
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarArticulos(int id,
            [FromBody] List<ActualizarArticuloDto> articulos)
        {
            var entrega = await _db.EntregasEPP
                .Include(x => x.Articulos)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entrega is null) return NotFound();

            foreach (var art in articulos)
            {
                var existente = entrega.Articulos
                    .FirstOrDefault(a => a.Id == art.Id);

                if (existente is not null)
                {
                    existente.Marca = art.Marca;
                    existente.FechaVencimiento = art.FechaVencimiento;
                    existente.Estado = art.Estado;
                }
            }

            entrega.FechaActualizacion = DateTime.UtcNow;

            // Regenerar hash con datos actualizados
            entrega.HashSha256 = _hash.GenerarHash(new
            {
                entrega.NombreTrabajador,
                entrega.CedulaTrabajador,
                entrega.FechaEntrega,
                FechaActualizacion = entrega.FechaActualizacion,
                articulos = entrega.Articulos.Select(a => new
                {
                    a.TipoEPP,
                    a.Marca,
                    a.FechaVencimiento,
                    a.Estado
                })
            });

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── GET /api/epp/{id}/pdf ──────────────────────────────
        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var entrega = await _db.EntregasEPP
                .AsNoTracking()
                .Include(x => x.Articulos)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entrega is null) return NotFound();

            var dto = MapearDetalle(entrega);
            var pdfBytes = _pdf.GenerarEntregaEPPPdf(dto);

            return File(pdfBytes, "application/pdf",
                $"EPP_{entrega.NombreTrabajador.Replace(" ", "_")}_" +
                $"{entrega.FechaEntrega:yyyyMMdd}.pdf");
        }

        // ── GET /api/epp/export ────────────────────────────────
        [HttpGet("export")]
        public async Task<IActionResult> ExportarCsv(
            [FromQuery] string? cuadrilla = null)
        {
            var query = _db.EntregasEPP
                .AsNoTracking()
                .Include(x => x.Articulos)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cuadrilla))
                query = query.Where(x => x.Cuadrilla == cuadrilla);

            var entregas = await query
                .OrderByDescending(x => x.FechaEntrega)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Trabajador,Cédula,Cargo,Cuadrilla,Obra," +
                          "Fecha entrega,Artículos,Estado general," +
                          "Firmado,Hash SHA-256");

            foreach (var e in entregas)
            {
                var estado = DeterminarEstadoGeneral(e.Articulos);
                sb.AppendLine(string.Join(",",
                    EscaparCsv(e.NombreTrabajador),
                    EscaparCsv(e.CedulaTrabajador),
                    EscaparCsv(e.Cargo),
                    EscaparCsv(e.Cuadrilla),
                    EscaparCsv(e.Obra),
                    e.FechaEntrega.ToString("dd/MM/yyyy"),
                    e.Articulos.Count,
                    EscaparCsv(estado),
                    e.Firmado ? "Sí" : "No",
                    e.HashSha256 ?? ""
                ));
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();

            return File(bytes, "text/csv",
                $"EPP_{DateTime.Now:yyyyMMdd}.csv");
        }

        // ── GET /api/epp/alertas ───────────────────────────────
        [HttpGet("alertas")]
        public async Task<ActionResult<List<AlertaEPPDto>>> GetAlertas()
        {
            var hoy = DateTime.UtcNow.Date;
            var en30Dias = hoy.AddDays(30);

            var articulos = await _db.ArticulosEPP
                .AsNoTracking()
                .Include(x => x.EntregaEPP)
                .Where(x =>
                    x.FechaVencimiento > DateTime.MinValue.AddDays(1) &&
                    (x.Estado == "Vencido" ||
                     (x.Estado == "Vigente" &&
                      x.FechaVencimiento <= en30Dias &&
                      x.FechaVencimiento >= hoy.AddYears(-1))))
                .OrderBy(x => x.FechaVencimiento)
                .ToListAsync();

            return Ok(articulos.Select(a => new AlertaEPPDto
            {
                NombreTrabajador = a.EntregaEPP?.NombreTrabajador ?? "",
                Cargo = a.EntregaEPP?.Cargo ?? "",
                Cuadrilla = a.EntregaEPP?.Cuadrilla ?? "",
                TipoEPP = a.TipoEPP,
                DiasRestantes = (int)(a.FechaVencimiento - hoy).TotalDays,
                Estado = a.Estado
            }).ToList());
        }

        // ── GET /api/epp/historial ─────────────────────────────
        [HttpGet("historial")]
        public async Task<ActionResult<List<HistorialEPPDto>>> GetHistorial()
        {
            var entregas = await _db.EntregasEPP
                .AsNoTracking()
                .Include(x => x.Articulos)
                .OrderByDescending(x => x.FechaEntrega)
                .Take(50)
                .ToListAsync();

            var resultado = new List<HistorialEPPDto>();

            foreach (var e in entregas)
            {
                foreach (var a in e.Articulos)
                {
                    resultado.Add(new HistorialEPPDto
                    {
                        NombreTrabajador = e.NombreTrabajador,
                        TipoEPP = a.TipoEPP,
                        Marca = a.Marca,
                        FechaEntrega = e.FechaEntrega,
                        FechaVencimiento = a.FechaVencimiento,
                        EntregadoPor = e.EntregadoPor,
                        Estado = a.Estado
                    });
                }
            }

            return Ok(resultado);
        }

        private static string EscaparCsv(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return "";
            if (valor.Contains(',') || valor.Contains('"') ||
                valor.Contains('\n'))
                return $"\"{valor.Replace("\"", "\"\"")}\"";
            return valor;
        }

        // ── Helpers ────────────────────────────────────────────
        private static string DeterminarEstadoGeneral(
            ICollection<ArticuloEPP> articulos)
        {
            if (!articulos.Any()) return "Sin EPP";
            if (articulos.Any(a => a.Estado == "Vencido"))
                return "Requiere acción";
            if (articulos.Any(a =>
                a.Estado == "Vigente" &&
                a.FechaVencimiento <= DateTime.UtcNow.AddDays(30)))
                return "Por vencer";
            return "Al día";
        }

        private static ArticuloEPPDto MapArticulo(ArticuloEPP a) => new()
        {
            Id = a.Id,
            TipoEPP = a.TipoEPP,
            Categoria = a.Categoria,
            Marca = a.Marca,
            FechaVencimiento = a.FechaVencimiento,
            Estado = a.Estado
        };

        private static EntregaEPPDetalleDto MapearDetalle(EntregaEPP x) => new()
        {
            Id = x.Id,
            NombreTrabajador = x.NombreTrabajador,
            CedulaTrabajador = x.CedulaTrabajador,
            Cargo = x.Cargo,
            Cuadrilla = x.Cuadrilla,
            Obra = x.Obra,
            FechaEntrega = x.FechaEntrega,
            EntregadoPor = x.EntregadoPor,
            Firmado = x.Firmado,
            HashSha256 = x.HashSha256,
            Articulos = x.Articulos.Select(MapArticulo).ToList()
        };
    }
}
