namespace SSTDigitalRD.Server.Services
{
    public interface IGeofencingService
    {
        bool EstaEnPerimetro(double latRegistro, double lngRegistro,
                             double latObra, double lngObra,
                             double radioMetros = 100);
    }

    public class GeofencingService : IGeofencingService
    {
        private const double RadioTierra = 6371000; // metros

        public bool EstaEnPerimetro(double latRegistro, double lngRegistro,
                                    double latObra, double lngObra,
                                    double radioMetros = 100)
        {
            var distancia = CalcularDistancia(latRegistro, lngRegistro,
                                              latObra, lngObra);
            return distancia <= radioMetros;
        }

        private static double CalcularDistancia(double lat1, double lng1,
                                                double lat2, double lng2)
        {
            var dLat = ToRad(lat2 - lat1);
            var dLng = ToRad(lng2 - lng1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return RadioTierra * c;
        }

        private static double ToRad(double grados) =>
            grados * Math.PI / 180;
    }
}
