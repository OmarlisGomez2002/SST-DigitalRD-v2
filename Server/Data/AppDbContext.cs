using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Models;

namespace SSTDigitalRD.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Inspeccion> Inspecciones { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        public DbSet<Charla> Charlas { get; set; }
        public DbSet<AsistenteCharla> AsistentesCharla { get; set; }
        public DbSet<EntregaEPP> EntregasEPP { get; set; }
        public DbSet<ArticuloEPP> ArticulosEPP { get; set; }
        public DbSet<Incidente> Incidentes { get; set; }
        public DbSet<AccionCorrectiva> AccionesCorrectivas { get; set; }
        public DbSet<ConfiguracionEmpresa> ConfiguracionEmpresa { get; set; }
        public DbSet<ObraActiva> ObrasActivas { get; set; }
        public DbSet<NotificacionConfig> NotificacionesConfig { get; set; }
        public DbSet<UsuarioSistema> UsuariosSistema { get; set; }
        public DbSet<Empleado> Empleados { get; set; }

        public DbSet<TipoInspeccion> TiposInspeccion { get; set; }

        public DbSet<Cuadrilla> Cuadrillas { get; set; }
        public DbSet<TipoCharla> TiposCharla { get; set; }

        public DbSet<CargoEmpleado> CargosEmpleado { get; set; }

        public DbSet<CapturaVision> CapturasVision { get; set; }

        public DbSet<ItemChecklist> ItemsChecklist { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<Inspeccion>(e =>
            {
                e.ToTable("Inspecciones");
                e.HasIndex(x => x.FechaInspeccion);
                e.HasIndex(x => x.Estado);
                e.HasIndex(x => x.Inspector);
            });

            mb.Entity<ChecklistItem>(e =>
            {
                e.ToTable("ChecklistItems");
                e.HasOne(x => x.Inspeccion)
                 .WithMany(x => x.Items)
                 .HasForeignKey(x => x.InspeccionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<Charla>(e =>
            {
                e.ToTable("Charlas");
                e.HasIndex(x => x.FechaCharla);
                e.HasIndex(x => x.Instructor);
                e.HasIndex(x => x.Obra);
            });

            mb.Entity<AsistenteCharla>(e =>
            {
                e.ToTable("AsistentesCharla");
                e.HasOne(x => x.Charla)
                 .WithMany(x => x.Asistentes)
                 .HasForeignKey(x => x.CharlaId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<EntregaEPP>(e =>
            {
                e.ToTable("EntregasEPP");
                e.HasIndex(x => x.CedulaTrabajador);
                e.HasIndex(x => x.FechaEntrega);
                e.HasIndex(x => x.Obra);
            });

            mb.Entity<ArticuloEPP>(e =>
            {
                e.ToTable("ArticulosEPP");
                e.HasIndex(x => x.Estado);
                e.HasIndex(x => x.FechaVencimiento);
                e.HasOne(x => x.EntregaEPP)
                 .WithMany(x => x.Articulos)
                 .HasForeignKey(x => x.EntregaEPPId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<Incidente>(e =>
            {
                e.ToTable("Incidentes");
                e.HasIndex(x => x.FechaIncidente);
                e.HasIndex(x => x.Tipo);
                e.HasIndex(x => x.Estado);
                e.HasIndex(x => x.Obra);
            });

            mb.Entity<AccionCorrectiva>(e =>
            {
                e.ToTable("AccionesCorrectivas");
                e.HasIndex(x => x.Estado);
                e.HasOne(x => x.Incidente)
                 .WithMany(x => x.AccionesCorrectivas)
                 .HasForeignKey(x => x.IncidenteId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<ConfiguracionEmpresa>(e =>
            {
                e.ToTable("ConfiguracionEmpresa");
            });

            mb.Entity<ObraActiva>(e =>
            {
                e.ToTable("ObrasActivas");
                e.HasIndex(x => x.Activa);
            });

            mb.Entity<NotificacionConfig>(e =>
            {
                e.ToTable("NotificacionesConfig");
            });

            mb.Entity<UsuarioSistema>(e =>
            {
                e.ToTable("UsuariosSistema");
                e.HasIndex(x => x.Correo).IsUnique();
                e.HasIndex(x => x.Rol);
            });

            mb.Entity<Empleado>(e =>
            {
                e.ToTable("Empleados");
                e.HasIndex(x => x.Cedula).IsUnique();
                e.HasIndex(x => x.Cuadrilla);
                e.HasIndex(x => x.Estado);
                e.HasIndex(x => x.Obra);
            });

            mb.Entity<TipoInspeccion>(e =>
            {
                e.ToTable("TiposInspeccion");
                e.HasIndex(x => x.Nombre).IsUnique();
            });

            mb.Entity<Cuadrilla>(e =>
            {
                e.ToTable("Cuadrillas");
                e.HasIndex(x => x.Nombre).IsUnique();
            });

            mb.Entity<TipoCharla>(e =>
            {
                e.ToTable("TiposCharla");
                e.HasIndex(x => x.Nombre).IsUnique();
            });

            mb.Entity<CargoEmpleado>(e =>
            {
                e.ToTable("CargosEmpleado");
                e.HasIndex(x => x.Nombre).IsUnique();
            });

            mb.Entity<ItemChecklist>(e =>
            {
                e.ToTable("ItemsChecklist");
                e.HasIndex(x => x.Categoria);
                e.HasIndex(x => x.Activo);
            });
        }
    }
}
