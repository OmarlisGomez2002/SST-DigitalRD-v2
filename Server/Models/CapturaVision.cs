namespace SSTDigitalRD.Server.Models
{
    public class CapturaVision
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Area { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public bool TieneInfraccion { get; set; }
        public string HashSha256 { get; set; } = "";
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public int PctCasco { get; set; }
        public int PctChaleco { get; set; }
        public int PctBotas { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
