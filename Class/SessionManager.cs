using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SintomApp.Class
{
    public static class SessionManager
    {
        public static Usuario UsuarioActual { get; set; }
        public static Usuario UsuarioSeleccionado { get; set; }
        public static Pregunta PreguntaSeleccionada { get; set; }
        public static DateTime FechaSeleccionada { get; set; }
        public static string HtmlRespuestas { get; set; }
        public static bool EsEdicion { get; set; }
        public static string TituloPagina { get; set; }
    }
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? Telefono { get; set; }
        public string? Apellidos { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? PrimerDia { get; set; }
        public int? Frecuencia { get; set; }
        public string? Patologia { get; set; }
        public int Admin { get; set; }
        public int? Medico { get; set; }
        public bool? Enable { get; set; } = true;
        public DateOnly? Nacimiento { get; set; }
        public string? Token { get; set; } // Para el registro
        public bool? Usado { get; set; } // Para el registro
        public DateTime? Fecha_generacion { get; set; } // Para el registro
        public int? Edad { get; set; } // Calculada a partir de la fecha de nacimiento

    }

    public class Pregunta
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public string? Texto2 { get; set; }
        public string? Texto3 { get; set; }
        public string? Tipo { get; set; }
        public decimal? Puntuacion { get; set; }
        public int? PuntuacionPersistente { get; set; }
        public int? PuntuacionNueva { get; set; }
        public string? PatologiaAsociada { get; set; }

    }

}
