using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Server.Models;
using SSTDigitalRD.Shared.DTOs;

namespace SSTDigitalRD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpleadosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EmpleadosController(AppDbContext db) => _db = db;

        // ── GET /api/empleados ─────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<List<EmpleadoListDto>>> GetTodos(
            [FromQuery] string? cuadrilla = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? buscar = null,
            [FromQuery] string? obra = null)
        {
            var query = _db.Empleados.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(cuadrilla))
                query = query.Where(x => x.Cuadrilla == cuadrilla);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(x => x.Estado == estado);

            if (!string.IsNullOrEmpty(buscar))
                query = query.Where(x =>
                    x.Nombre.Contains(buscar) ||
                    x.Cedula.Contains(buscar) ||
                    x.Cargo.Contains(buscar));
            
            if (!string.IsNullOrEmpty(obra))    
                query = query.Where(x => x.Obra == obra);

            var lista = await query
                .OrderBy(x => x.Nombre)
                .Select(x => new EmpleadoListDto
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    Cedula = x.Cedula,
                    Cargo = x.Cargo,
                    Cuadrilla = x.Cuadrilla,
                    Obra = x.Obra,
                    Estado = x.Estado
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ── GET /api/empleados/{id} ────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmpleadoDetalleDto>> GetPorId(int id)
        {
            var emp = await _db.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (emp is null) return NotFound();
            return Ok(MapearDetalle(emp));
        }

        // ── GET /api/empleados/cedula/{cedula} ─────────────────
        [HttpGet("cedula/{cedula}")]
        public async Task<ActionResult<EmpleadoDetalleDto>> GetPorCedula(
            string cedula)
        {
            var emp = await _db.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Cedula == cedula);

            if (emp is null) return NotFound();
            return Ok(MapearDetalle(emp));
        }

        // ── POST /api/empleados ────────────────────────────────
        [HttpPost]
        public async Task<ActionResult<EmpleadoDetalleDto>> Crear(
            [FromBody] CrearEmpleadoDto dto)
        {
            // Verificar cédula duplicada
            var existe = await _db.Empleados
                .AnyAsync(x => x.Cedula == dto.Cedula);

            if (existe)
                return Conflict(new
                {
                    error = $"Ya existe un empleado con la cédula {dto.Cedula}."
                });

            var emp = new Empleado
            {
                Nombre = dto.Nombre,
                Cedula = dto.Cedula,
                FechaNacimiento = dto.FechaNacimiento,
                Telefono = dto.Telefono,
                Correo = dto.Correo,
                Direccion = dto.Direccion,
                Cargo = dto.Cargo,
                Cuadrilla = dto.Cuadrilla,
                Obra = dto.Obra,
                FechaIngreso = dto.FechaIngreso,
                TipoContrato = dto.TipoContrato,
                NumeroTSS = dto.NumeroTSS,
                Estado = dto.Estado
            };

            _db.Empleados.Add(emp);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPorId),
                new { id = emp.Id }, MapearDetalle(emp));
        }

        // ── PUT /api/empleados/{id} ────────────────────────────
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id,
            [FromBody] CrearEmpleadoDto dto)
        {
            var emp = await _db.Empleados.FindAsync(id);
            if (emp is null) return NotFound();

            emp.Nombre = dto.Nombre;
            emp.FechaNacimiento = dto.FechaNacimiento;
            emp.Telefono = dto.Telefono;
            emp.Correo = dto.Correo;
            emp.Direccion = dto.Direccion;
            emp.Cargo = dto.Cargo;
            emp.Cuadrilla = dto.Cuadrilla;
            emp.TipoContrato = dto.TipoContrato;
            emp.NumeroTSS = dto.NumeroTSS;
            emp.Estado = dto.Estado;
            emp.FechaActualizacion = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── PUT /api/empleados/{id}/desactivar ─────────────────
        [HttpPut("{id:int}/desactivar")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var emp = await _db.Empleados.FindAsync(id);
            if (emp is null) return NotFound();

            emp.Estado = "Inactivo";
            emp.FechaActualizacion = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── DELETE /api/empleados/{id} ─────────────────────────
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var emp = await _db.Empleados.FindAsync(id);
            if (emp is null) return NotFound();

            _db.Empleados.Remove(emp);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── Helper ─────────────────────────────────────────────
        private static EmpleadoDetalleDto MapearDetalle(Empleado x) => new()
        {
            Id = x.Id,
            Nombre = x.Nombre,
            Cedula = x.Cedula,
            FechaNacimiento = x.FechaNacimiento,
            Telefono = x.Telefono,
            Correo = x.Correo,
            Direccion = x.Direccion,
            Cargo = x.Cargo,
            Cuadrilla = x.Cuadrilla,
            Obra = x.Obra,
            FechaIngreso = x.FechaIngreso,
            TipoContrato = x.TipoContrato,
            NumeroTSS = x.NumeroTSS,
            Estado = x.Estado
        };
    }
}
