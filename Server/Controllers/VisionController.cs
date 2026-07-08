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
    public class VisionController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHashService _hash;
        private readonly IYoloService _yolo;

        public VisionController(AppDbContext db, IHashService hash, IYoloService yolo)
        {
            _db = db;
            _hash = hash;
            _yolo = yolo;
        }



        // ── GET /api/vision/capturas ───────────────────────────
        [HttpGet("capturas")]
        public async Task<ActionResult<List<CapturaVisionListDto>>>
            GetCapturas([FromQuery] string? area = null)
        {
            var hoy = DateTime.UtcNow.Date;
            var query = _db.CapturasVision
                .AsNoTracking()
                .Where(x => x.Timestamp.Date == hoy);

            if (!string.IsNullOrEmpty(area))
                query = query.Where(x => x.Area == area);

            var lista = await query
                .OrderByDescending(x => x.Timestamp)
                .Select(x => new CapturaVisionListDto
                {
                    Id = x.Id,
                    Timestamp = x.Timestamp,
                    Area = x.Area,
                    Descripcion = x.Descripcion,
                    TieneInfraccion = x.TieneInfraccion,
                    HashSha256 = x.HashSha256,
                    Latitud = x.Latitud,
                    Longitud = x.Longitud,
                    PctCasco = x.PctCasco,
                    PctChaleco = x.PctChaleco,
                    PctBotas = x.PctBotas
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ── GET /api/vision/alertas ────────────────────────────
        [HttpGet("alertas")]
        public async Task<ActionResult<List<CapturaVisionListDto>>>
            GetAlertas()
        {
            var hoy = DateTime.UtcNow.Date;
            var lista = await _db.CapturasVision
                .AsNoTracking()
                .Where(x => x.TieneInfraccion &&
                            x.Timestamp.Date == hoy)
                .OrderByDescending(x => x.Timestamp)
                .Select(x => new CapturaVisionListDto
                {
                    Id = x.Id,
                    Timestamp = x.Timestamp,
                    Area = x.Area,
                    Descripcion = x.Descripcion,
                    TieneInfraccion = x.TieneInfraccion,
                    HashSha256 = x.HashSha256,
                    Latitud = x.Latitud,
                    Longitud = x.Longitud,
                    PctCasco = x.PctCasco,
                    PctChaleco = x.PctChaleco,
                    PctBotas = x.PctBotas
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ── POST /api/vision/capturar ──────────────────────────
        [HttpPost("capturar")]
        public async Task<ActionResult<CapturaVisionListDto>>
            Capturar([FromBody] CrearCapturaVisionDto dto)
        {
            // Ejecutar inferencia YOLO si viene imagen
            YoloDeteccionResultado deteccion;

            if (!string.IsNullOrEmpty(dto.ImageBase64))
            {
                try
                {
                    var bytes = Convert.FromBase64String(
                        dto.ImageBase64.Contains(",")
                            ? dto.ImageBase64.Split(',')[1]
                            : dto.ImageBase64);
                    deteccion = _yolo.Detectar(bytes);
                }
                catch
                {
                    deteccion = new YoloDeteccionResultado
                    {
                        PctCasco = dto.PctCasco,
                        PctChaleco = dto.PctChaleco,
                        PctBotas = dto.PctBotas
                    };
                }
            }
            else
            {
                deteccion = new YoloDeteccionResultado
                {
                    PctCasco = dto.PctCasco,
                    PctChaleco = dto.PctChaleco,
                    PctBotas = dto.PctBotas
                };
            }

            var tieneInfraccion = deteccion.TienePersonaSinCasco ||
                                  deteccion.TienePersonaSinChaleco;
            var captura = new CapturaVision
            {
                Timestamp = DateTime.UtcNow,
                Area = dto.Area,
                Descripcion = deteccion.Descripcion.Length > 0 ? deteccion.Descripcion : dto.Descripcion,
                TieneInfraccion = dto.TieneInfraccion,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                PctCasco = dto.PctCasco,
                PctChaleco = dto.PctChaleco,
                PctBotas = dto.PctBotas
            };

            captura.HashSha256 = _hash.GenerarHash(new
            {
                captura.Area,
                captura.Timestamp,
                captura.Latitud,
                captura.Longitud,
                captura.TieneInfraccion,
                captura.PctCasco,
                captura.PctChaleco,
                captura.PctBotas
            });

            _db.CapturasVision.Add(captura);
            await _db.SaveChangesAsync();

            return Ok(new CapturaVisionListDto
            {
                Id = captura.Id,
                Timestamp = captura.Timestamp,
                Area = captura.Area,
                Descripcion = captura.Descripcion,
                TieneInfraccion = captura.TieneInfraccion,
                HashSha256 = captura.HashSha256,
                Latitud = captura.Latitud,
                Longitud = captura.Longitud,
                PctCasco = captura.PctCasco,
                PctChaleco = captura.PctChaleco,
                PctBotas = captura.PctBotas
            });
        }
    }
}
