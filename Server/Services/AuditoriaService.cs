using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Server.Models;

namespace SSTDigitalRD.Server.Services
{
    public interface IAuditoriaService
    {
        Task RegistrarAsync(string accion, string entidad, string detalle, int? usuarioId = null, string usuarioNombre = "", string ip = "");
    }

    public class AuditoriaService : IAuditoriaService
    {
        private readonly AppDbContext _db;

        public AuditoriaService(AppDbContext db) => _db = db;

        public async Task RegistrarAsync(string accion, string entidad, string detalle, int? usuarioId = null, string usuarioNombre = "", string ip = "")
        {
            _db.AuditoriaLogs.Add(new AuditoriaLog
            {
                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,
                Accion = accion,
                Entidad = entidad,
                Detalle = detalle,
                IpAddress = ip,
                Fecha = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }
}
