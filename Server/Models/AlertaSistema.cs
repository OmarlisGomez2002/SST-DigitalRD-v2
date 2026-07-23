using System.ComponentModel.DataAnnotations;

namespace SSTDigitalRD.Server.Models
{
    public class AlertaSistema
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Titulo { get; set; } = "";

        [MaxLength(300)]
        public string Descripcion { get; set; } = "";

        [MaxLength(50)]
        public string Tipo { get; set; } = "ia"; // ia, epp, incidente

        [MaxLength(50)]
        public string Nivel { get; set; } = "info"; // info, warn, danger

        [MaxLength(200)]
        public string Zona { get; set; } = "";

        public int NivelRiesgo { get; set; }
        public bool Leida { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
