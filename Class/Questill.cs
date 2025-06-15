
namespace SintomApp.Class
{
    public static class Questill
    { 
        
        public static bool FechaEsEncuesta(DateTime primerDia, int frecuencia, DateTime fecha)
        {
            var actual = primerDia.Date;
            while (actual <= fecha)
            {
                if (actual == fecha) return true;
                actual = actual.AddDays(frecuencia);
            }
            return false;
        }

    }
}
