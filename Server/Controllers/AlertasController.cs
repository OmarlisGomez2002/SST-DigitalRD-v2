using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Shared.DTOs;

namespace SSTDigitalRD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertasController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AlertasController(AppDbContext db) => _db = db;

        // GET /api/alertas — todas las alertas no leídas
        [HttpGet]
        public async Task<ActionResult<List<AlertaSistemaDto>>> GetPendientes(
            [FromQuery] bool soloNoLeidas = true)
        {
            var query = _db.AlertasSistema.AsNoTracking();

            if (soloNoLeidas)
                query = query.Where(x => !x.Leida);

            var lista = await query
                .OrderByDescending(x => x.FechaCreacion)
                .Take(20)
                .Select(x => new AlertaSistemaDto
                {
                    Id = x.Id,
                    Titulo = x.Titulo,
                    Descripcion = x.Descripcion,
                    Tipo = x.Tipo,
                    Nivel = x.Nivel,
                    Zona = x.Zona,
                    NivelRiesgo = x.NivelRiesgo,
                    Leida = x.Leida,
                    Fecha = x.FechaCreacion
                        .ToString("dd/MM/yyyy hh:mm tt")
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET /api/alertas/conteo — solo el número de no leídas
        [HttpGet("conteo")]
        public async Task<ActionResult<int>> GetConteo()
        {
            var conteo = await _db.AlertasSistema
                .CountAsync(x => !x.Leida);
            return Ok(conteo);
        }

        // PUT /api/alertas/{id}/leer — marcar como leída
        [HttpPut("{id:int}/leer")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var alerta = await _db.AlertasSistema.FindAsync(id);
            if (alerta is null) return NotFound();

            alerta.Leida = true;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PUT /api/alertas/leer-todas
        [HttpPut("leer-todas")]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var pendientes = await _db.AlertasSistema
                .Where(x => !x.Leida)
                .ToListAsync();

            foreach (var a in pendientes)
                a.Leida = true;

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
