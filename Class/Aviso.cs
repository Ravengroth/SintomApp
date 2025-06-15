using Microsoft.Maui.Graphics;

namespace SintomApp.Class
{
    public class Aviso
    {
        public int Id { get; set; }
        public int IdPaciente { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        public bool Leido { get; set; }

        // Propiedades auxiliares para la interfaz
        public string LeidoTexto { get; set; }
        public Color LeidoColor { get; set; }
    }
}