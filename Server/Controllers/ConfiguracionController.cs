using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Server.Models;
using SSTDigitalRD.Shared.DTOs;

namespace SSTDigitalRD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracionController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ConfiguracionController(AppDbContext db) => _db = db;

        // ══ EMPRESA ════════════════════════════════════════════

        // ── GET /api/configuracion/empresa ─────────────────────
        [HttpGet("empresa")]
        public async Task<ActionResult<ConfiguracionEmpresaDto>> GetEmpresa()
        {
            var config = await _db.ConfiguracionEmpresa
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (config is null)
            {
                // Crear configuración por defecto si no existe
                config = new ConfiguracionEmpresa
                {
                    RazonSocial = "Constructora Piantini S.R.L.",
                    RNC = "1-31-12345-6",
                    Sector = "Construcción privada — Edificación vertical",
                    ResponsableSST = "Ramón Gómez",
                    Telefono = "809-000-0000",
                    Correo = "sst@piantini.com.do"
                };
                _db.ConfiguracionEmpresa.Add(config);
                await _db.SaveChangesAsync();
            }

            return Ok(new ConfiguracionEmpresaDto
            {
                Id = config.Id,
                RazonSocial = config.RazonSocial,
                RNC = config.RNC,
                Sector = config.Sector,
                ResponsableSST = config.ResponsableSST,
                Telefono = config.Telefono,
                Correo = config.Correo
            });
        }

        // ── PUT /api/configuracion/empresa ─────────────────────
        [HttpPut("empresa")]
        public async Task<IActionResult> ActualizarEmpresa(
            [FromBody] ConfiguracionEmpresaDto dto)
        {
            var config = await _db.ConfiguracionEmpresa.FirstOrDefaultAsync();

            if (config is null)
            {
                config = new ConfiguracionEmpresa();
                _db.ConfiguracionEmpresa.Add(config);
            }

            config.RazonSocial = dto.RazonSocial;
            config.RNC = dto.RNC;
            config.Sector = dto.Sector;
            config.ResponsableSST = dto.ResponsableSST;
            config.Telefono = dto.Telefono;
            config.Correo = dto.Correo;
            config.FechaActualizacion = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ══ OBRA ACTIVA ════════════════════════════════════════

        // ── GET /api/configuracion/obra ────────────────────────
        [HttpGet("obra")]
        public async Task<ActionResult<ObraActivaDto>> GetObra()
        {
            var obra = await _db.ObrasActivas
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Activa);

            if (obra is null)
            {
                obra = new ObraActiva
                {
                    Nombre = "Torre Piantini IV",
                    Direccion = "Av. Abraham Lincoln, Piantini, D.N.",
                    Latitud = 18.473621,
                    Longitud = -69.931215,
                    RadioGeofencing = 100,
                    Activa = true
                };
                _db.ObrasActivas.Add(obra);
                await _db.SaveChangesAsync();
            }

            return Ok(new ObraActivaDto
            {
                Id = obra.Id,
                Nombre = obra.Nombre,
                Direccion = obra.Direccion,
                Latitud = obra.Latitud,
                Longitud = obra.Longitud,
                RadioGeofencing = obra.RadioGeofencing,
                Activa = obra.Activa
            });
        }

        // ── GET /api/configuracion/obras ── (todas las obras activas)
        [HttpGet("obras")]
        public async Task<ActionResult<List<ObraActivaDto>>> GetObras()
        {
            var obras = await _db.ObrasActivas
                .AsNoTracking()
                .Where(x => x.Activa)
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            // Si no hay ninguna crear la de pruebas por defecto
            if (!obras.Any())
            {
                var obraDefault = new ObraActiva
                {
                    Nombre = "Invivienda — Edificio Central",
                    Direccion = "Av. Simón Orozco, D.N.",
                    Latitud = 18.515657,
                    Longitud = -69.825995,
                    RadioGeofencing = 50000,
                    Activa = true
                };
                _db.ObrasActivas.Add(obraDefault);
                await _db.SaveChangesAsync();
                obras = new List<ObraActiva> { obraDefault };
            }

            return Ok(obras.Select(x => new ObraActivaDto
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Direccion = x.Direccion,
                Latitud = x.Latitud,
                Longitud = x.Longitud,
                RadioGeofencing = x.RadioGeofencing,
                Activa = x.Activa
            }).ToList());
        }

        // ── POST /api/configuracion/obras ── (agregar nueva obra)
        [HttpPost("obras")]
        public async Task<ActionResult<ObraActivaDto>> AgregarObra(
            [FromBody] ObraActivaDto dto)
        {
            var obra = new ObraActiva
            {
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                RadioGeofencing = dto.RadioGeofencing,
                Activa = true
            };

            _db.ObrasActivas.Add(obra);
            await _db.SaveChangesAsync();

            return Ok(new ObraActivaDto
            {
                Id = obra.Id,
                Nombre = obra.Nombre,
                Direccion = obra.Direccion,
                Latitud = obra.Latitud,
                Longitud = obra.Longitud,
                RadioGeofencing = obra.RadioGeofencing,
                Activa = obra.Activa
            });
        }

        // ── PUT /api/configuracion/obra ────────────────────────
        [HttpPut("obra")]
        public async Task<IActionResult> ActualizarObra(
            [FromBody] ObraActivaDto dto)
        {
            var obra = await _db.ObrasActivas
                .FirstOrDefaultAsync(x => x.Activa);

            if (obra is null)
            {
                obra = new ObraActiva();
                _db.ObrasActivas.Add(obra);
            }

            obra.Nombre = dto.Nombre;
            obra.Direccion = dto.Direccion;
            obra.Latitud = dto.Latitud;
            obra.Longitud = dto.Longitud;
            obra.RadioGeofencing = dto.RadioGeofencing;
            obra.FechaActualizacion = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ══ NOTIFICACIONES ═════════════════════════════════════

        // ── GET /api/configuracion/notificaciones ──────────────
        [HttpGet("notificaciones")]
        public async Task<ActionResult<List<NotificacionConfigDto>>>
            GetNotificaciones()
        {
            var lista = await _db.NotificacionesConfig
                .AsNoTracking()
                .ToListAsync();

            if (!lista.Any())
            {
                // Crear notificaciones por defecto
                var defaults = new List<NotificacionConfig>
            {
                new() { Titulo="Alertas de EPP vencido",
                        Descripcion="Notificar 30 días antes del vencimiento",
                        Activa=true },
                new() { Titulo="Infracción detectada por IA",
                        Descripcion="Alerta inmediata al prevencionista",
                        Activa=true },
                new() { Titulo="Zona de riesgo crítico",
                        Descripcion="Notificar cuando riesgo supere 70%",
                        Activa=true },
                new() { Titulo="Reporte semanal de dossier",
                        Descripcion="Resumen automático los lunes",
                        Activa=false },
                new() { Titulo="Accidente grave registrado",
                        Descripcion="Notificación urgente al responsable SST",
                        Activa=true },
                new() { Titulo="Inspección pendiente",
                        Descripcion="Recordatorio 24h antes del vencimiento",
                        Activa=true },
            };
                _db.NotificacionesConfig.AddRange(defaults);
                await _db.SaveChangesAsync();
                lista = defaults;
            }

            return Ok(lista.Select(x => new NotificacionConfigDto
            {
                Id = x.Id,
                Titulo = x.Titulo,
                Descripcion = x.Descripcion,
                Activa = x.Activa
            }).ToList());
        }

        // ── PUT /api/configuracion/notificaciones ──────────────
        [HttpPut("notificaciones")]
        public async Task<IActionResult> ActualizarNotificaciones(
            [FromBody] List<NotificacionConfigDto> dto)
        {
            foreach (var item in dto)
            {
                var noti = await _db.NotificacionesConfig.FindAsync(item.Id);
                if (noti is null) continue;

                noti.Activa = item.Activa;
                noti.FechaActualizacion = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ══ USUARIOS ═══════════════════════════════════════════

        // ── GET /api/configuracion/usuarios ───────────────────
        [HttpGet("usuarios")]
        public async Task<ActionResult<List<UsuarioSistemaDto>>>
            GetUsuarios()
        {
            var lista = await _db.UsuariosSistema
                .AsNoTracking()
                .Where(x => x.Activo)
                .OrderBy(x => x.Nombre)
                .Select(x => new UsuarioSistemaDto
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    Correo = x.Correo,
                    Rol = x.Rol,
                    Cuadrilla = x.Cuadrilla,
                    Activo = x.Activo
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ── POST /api/configuracion/usuarios ──────────────────
        [HttpPost("usuarios")]
        public async Task<ActionResult<UsuarioSistemaDto>> CrearUsuario(
            [FromBody] CrearUsuarioDto dto)
        {
            var existe = await _db.UsuariosSistema
                .AnyAsync(x => x.Correo == dto.Correo);

            if (existe)
                return Conflict(new
                {
                    error = $"Ya existe un usuario con el correo {dto.Correo}."
                });

            var usuario = new UsuarioSistema
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                Rol = dto.Rol,
                Cuadrilla = dto.Cuadrilla,
                Activo = true
            };

            _db.UsuariosSistema.Add(usuario);
            await _db.SaveChangesAsync();

            return Ok(new UsuarioSistemaDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Cuadrilla = usuario.Cuadrilla,
                Activo = usuario.Activo
            });
        }

        // ── DELETE /api/configuracion/usuarios/{id} ───────────
        [HttpDelete("usuarios/{id:int}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _db.UsuariosSistema.FindAsync(id);
            if (usuario is null) return NotFound();

            usuario.Activo = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ══ TIPOS DE INSPECCIÓN ════════════════════════════════

        // ── GET /api/configuracion/tipos-inspeccion ────────────
        [HttpGet("tipos-inspeccion")]
        public async Task<ActionResult<List<TipoInspeccionDto>>>
            GetTiposInspeccion()
        {
            var lista = await _db.TiposInspeccion
                .AsNoTracking()
                .Where(x => x.Activo)
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            if (!lista.Any())
            {
                var defaults = new List<TipoInspeccion>
        {
            new() { Nombre = "Inspección de rutina" },
            new() { Nombre = "Inspección de EPP" },
            new() { Nombre = "Inspección post-incidente" },
            new() { Nombre = "Inspección de andamios" },
            new() { Nombre = "Inspección eléctrica" },
            new() { Nombre = "Inspección contra incendios" },
        };
                _db.TiposInspeccion.AddRange(defaults);
                await _db.SaveChangesAsync();
                lista = defaults;
            }

            return Ok(lista.Select(x => new TipoInspeccionDto
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Activo = x.Activo
            }).ToList());
        }

        // ── POST /api/configuracion/tipos-inspeccion ───────────
        [HttpPost("tipos-inspeccion")]
        public async Task<ActionResult<TipoInspeccionDto>> AgregarTipoInspeccion(
            [FromBody] TipoInspeccionDto dto)
        {
            var existe = await _db.TiposInspeccion
                .AnyAsync(x => x.Nombre == dto.Nombre);

            if (existe)
                return Conflict(new
                {
                    error = $"Ya existe el tipo de inspección '{dto.Nombre}'."
                });

            var tipo = new TipoInspeccion { Nombre = dto.Nombre, Activo = true };
            _db.TiposInspeccion.Add(tipo);
            await _db.SaveChangesAsync();

            return Ok(new TipoInspeccionDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre,
                Activo = tipo.Activo
            });
        }

        // ── DELETE /api/configuracion/tipos-inspeccion/{id} ────
        [HttpDelete("tipos-inspeccion/{id:int}")]
        public async Task<IActionResult> EliminarTipoInspeccion(int id)
        {
            var tipo = await _db.TiposInspeccion.FindAsync(id);
            if (tipo is null) return NotFound();

            tipo.Activo = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ══ CUADRILLAS ═════════════════════════════════════════

        // ── GET /api/configuracion/cuadrillas ──────────────────
        [HttpGet("cuadrillas")]
        public async Task<ActionResult<List<CuadrillaDto>>> GetCuadrillas()
        {
            var lista = await _db.Cuadrillas
                .AsNoTracking()
                .Where(x => x.Activa)
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            if (!lista.Any())
            {
                var defaults = new List<Cuadrilla>
        {
            new() { Nombre = "Cuadrilla A" },
            new() { Nombre = "Cuadrilla B" },
            new() { Nombre = "Cuadrilla C" },
            new() { Nombre = "Cuadrilla D" },
        };
                _db.Cuadrillas.AddRange(defaults);
                await _db.SaveChangesAsync();
                lista = defaults;
            }

            return Ok(lista.Select(x => new CuadrillaDto
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Activa = x.Activa
            }).ToList());
        }

        // ── POST /api/configuracion/cuadrillas ─────────────────
        [HttpPost("cuadrillas")]
        public async Task<ActionResult<CuadrillaDto>> AgregarCuadrilla(
            [FromBody] CuadrillaDto dto)
        {
            var existe = await _db.Cuadrillas
                .AnyAsync(x => x.Nombre == dto.Nombre);

            if (existe)
                return Conflict(new
                {
                    error = $"Ya existe la cuadrilla '{dto.Nombre}'."
                });

            var cuadrilla = new Cuadrilla { Nombre = dto.Nombre, Activa = true };
            _db.Cuadrillas.Add(cuadrilla);
            await _db.SaveChangesAsync();

            return Ok(new CuadrillaDto
            {
                Id = cuadrilla.Id,
                Nombre = cuadrilla.Nombre,
                Activa = cuadrilla.Activa
            });
        }

        // ── DELETE /api/configuracion/cuadrillas/{id} ──────────
        [HttpDelete("cuadrillas/{id:int}")]
        public async Task<IActionResult> EliminarCuadrilla(int id)
        {
            var cuadrilla = await _db.Cuadrillas.FindAsync(id);
            if (cuadrilla is null) return NotFound();

            cuadrilla.Activa = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ══ TIPOS DE CHARLA ════════════════════════════════════

        [HttpGet("tipos-charla")]
        public async Task<ActionResult<List<TipoCharlaDto>>> GetTiposCharla()
        {
            var lista = await _db.TiposCharla
                .AsNoTracking()
                .Where(x => x.Activo)
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            if (!lista.Any())
            {
                var defaults = new List<TipoCharla>
        {
            new() { Nombre = "Charla diaria (toolbox talk)" },
            new() { Nombre = "Inducción de seguridad"       },
            new() { Nombre = "Capacitación especial"        },
            new() { Nombre = "Post-incidente"               },
            new() { Nombre = "Normativa legal"              },
        };
                _db.TiposCharla.AddRange(defaults);
                await _db.SaveChangesAsync();
                lista = defaults;
            }

            return Ok(lista.Select(x => new TipoCharlaDto
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Activo = x.Activo
            }).ToList());
        }

        [HttpPost("tipos-charla")]
        public async Task<ActionResult<TipoCharlaDto>> AgregarTipoCharla(
            [FromBody] TipoCharlaDto dto)
        {
            var existe = await _db.TiposCharla
                .AnyAsync(x => x.Nombre == dto.Nombre);

            if (existe)
                return Conflict(new
                {
                    error = $"Ya existe el tipo '{dto.Nombre}'."
                });

            var tipo = new TipoCharla { Nombre = dto.Nombre, Activo = true };
            _db.TiposCharla.Add(tipo);
            await _db.SaveChangesAsync();

            return Ok(new TipoCharlaDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre,
                Activo = tipo.Activo
            });
        }

        [HttpDelete("tipos-charla/{id:int}")]
        public async Task<IActionResult> EliminarTipoCharla(int id)
        {
            var tipo = await _db.TiposCharla.FindAsync(id);
            if (tipo is null) return NotFound();

            tipo.Activo = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ══ CARGOS DE EMPLEADO ═════════════════════════════════

        [HttpGet("cargos")]
        public async Task<ActionResult<List<CargoEmpleadoDto>>> GetCargos()
        {
            var lista = await _db.CargosEmpleado
                .AsNoTracking()
                .Where(x => x.Activo)
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            if (!lista.Any())
            {
                var defaults = new List<CargoEmpleado>
        {
            new() { Nombre = "Albañil"      },
            new() { Nombre = "Ayudante"     },
            new() { Nombre = "Carpintero"   },
            new() { Nombre = "Electricista" },
            new() { Nombre = "Operador"     },
            new() { Nombre = "Plomero"      },
            new() { Nombre = "Soldador"     },
            new() { Nombre = "Supervisor"   },
        };
                _db.CargosEmpleado.AddRange(defaults);
                await _db.SaveChangesAsync();
                lista = defaults;
            }

            return Ok(lista.Select(x => new CargoEmpleadoDto
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Activo = x.Activo
            }).ToList());
        }

        [HttpPost("cargos")]
        public async Task<ActionResult<CargoEmpleadoDto>> AgregarCargo(
            [FromBody] CargoEmpleadoDto dto)
        {
            var existe = await _db.CargosEmpleado
                .AnyAsync(x => x.Nombre == dto.Nombre);

            if (existe)
                return Conflict(new
                {
                    error = $"Ya existe el cargo '{dto.Nombre}'."
                });

            var cargo = new CargoEmpleado
            {
                Nombre = dto.Nombre,
                Activo = true
            };
            _db.CargosEmpleado.Add(cargo);
            await _db.SaveChangesAsync();

            return Ok(new CargoEmpleadoDto
            {
                Id = cargo.Id,
                Nombre = cargo.Nombre,
                Activo = cargo.Activo
            });
        }

        [HttpDelete("cargos/{id:int}")]
        public async Task<IActionResult> EliminarCargo(int id)
        {
            var cargo = await _db.CargosEmpleado.FindAsync(id);
            if (cargo is null) return NotFound();

            cargo.Activo = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }


    }
}
