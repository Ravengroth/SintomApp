using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using SintomApp.Class;
using System.Windows.Input;

namespace SintomApp.ViewModels
{
    public class PreguntaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _indiceActual;
        private bool _botonHabilitado;
        private string _textoBoton;
        private Pregunta? _preguntaActual;
        private decimal? _puntuacion;

        public ObservableCollection<Pregunta> Preguntas { get; set; } = new();
        public Dictionary<int, string> Respuestas { get; set; } = new();
        public Usuario Usuario { get; set; }

        public ICommand SiguienteCommand { get; }

        public PreguntaViewModel(Usuario user)
        {
            Usuario = user;
            SiguienteCommand = new Command(async () => await SiguienteAsync());
            _puntuacion = 0;
        }

        public async Task CargarPreguntasAsync()
        {
            var db = new DatabaseManager();
            var lista = await db.ObtenerPreguntasPorId(Usuario);
            Preguntas.Clear();
            foreach (var p in lista)
                Preguntas.Add(p);

            IndiceActual = 0;
            PreguntaActual = Preguntas.FirstOrDefault();
            ActualizarTextoBoton();
        }

        public int IndiceActual
        {
            get => _indiceActual;
            set
            {
                _indiceActual = value;
                OnPropertyChanged();
            }
        }

        public Pregunta? PreguntaActual
        {
            get => _preguntaActual;
            set
            {
                _preguntaActual = value;
                OnPropertyChanged();
            }
        }

        public bool BotonHabilitado
        {
            get => _botonHabilitado;
            set
            {
                _botonHabilitado = value;
                OnPropertyChanged();
            }
        }

        public string TextoBoton
        {
            get => _textoBoton;
            set
            {
                _textoBoton = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Registra la respuesta actual en formato JSON.
        /// </summary>
        /// <param name="idPregunta">ID de la pregunta</param>
        /// <param name="respuestaPrincipal">Sí o No</param>
        /// <param name="extra1">Dato adicional si aplica (string o número)</param>
        /// <param name="extra2">Segundo dato adicional si aplica</param>
        /// <param name="puntuacion">Puntuación asignada a esta pregunta</param>
        public void RegistrarRespuesta(int idPregunta, string respuestaPrincipal, object? extra1 = null, string? extra2 = null, decimal? puntuacion = 0)
        {
            var pregunta = Preguntas.FirstOrDefault(p => p.Id == idPregunta);
            if (pregunta == null)
                return;

            var respuestaDict = new Dictionary<string, object>
            {
                ["principal"] = respuestaPrincipal
            };

            if (respuestaPrincipal.ToLower() == "sí")
            {
                switch (pregunta.Tipo)
                {
                    case null:
                        if (extra1 is string strDetalle && !string.IsNullOrEmpty(strDetalle))
                            respuestaDict["detalle"] = strDetalle;
                        break;

                    case "int":
                        if (extra1 is int numero)
                            respuestaDict["extra1"] = numero;
                        else if (extra1 is string strNumero && int.TryParse(strNumero, out int n))
                            respuestaDict["extra1"] = n;
                        break;

                    case "string2":
                        if (extra1 is string s1 && !string.IsNullOrEmpty(s1))
                            respuestaDict["extra1"] = s1;
                        break;

                    case "string3":
                        if (extra1 is string s2 && !string.IsNullOrEmpty(s2))
                            respuestaDict["extra1"] = s2;
                        if (!string.IsNullOrEmpty(extra2))
                            respuestaDict["extra2"] = extra2;
                        break;
                }
            }
            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            Respuestas[idPregunta] = JsonSerializer.Serialize(respuestaDict, options);
            _puntuacion += puntuacion ?? 0;
            BotonHabilitado = true;
        }




        public bool EsUltimaPregunta() => IndiceActual >= Preguntas.Count - 1;

        public bool AvanzarPregunta()
        {
            if (!EsUltimaPregunta())
            {
                ActualizarTextoBoton();
                return true;
            }
            return false;
        }

        public int ObtenerLimitePorPatologia(string idPatologia)
        {
            return idPatologia switch
            {
                "LES" => 35,
                "Miositis" => 28,
                "Esclerodermia" => 30,
                "Vasculitis" => 32,
                _ => 30,
            };
        }


        public async Task<bool> GuardarRespuestasAsync()
        {
            string idPatologia = Usuario.Patologia;
            decimal? pTotal = _puntuacion;
            int limite = ObtenerLimitePorPatologia(idPatologia);
            var db = new DatabaseManager();
            if(pTotal.HasValue && pTotal.Value < limite) 
            {
                if (Usuario.Medico.HasValue)
                {                
                    await db.GuardarAvisoAsync(Usuario.Id, Usuario.Medico.Value, pTotal.Value, "Puntuación insuficiente para completar el cuestionario.");


                }
            }
            bool guardado = await db.GuardarRespuestasBD(Respuestas, Usuario.Id, _puntuacion);
            return guardado;
        }

        private async Task SiguienteAsync()
        {
            if (!EsUltimaPregunta())
            {
                IndiceActual++; // mueve primero el índice
                PreguntaActual = Preguntas[IndiceActual];
                BotonHabilitado = false;
                ActualizarTextoBoton();
            }
            else
            {
                var resultado = await GuardarRespuestasAsync();
                if (resultado)
                {
                    await Shell.Current.DisplayAlert("Éxito", "Respuestas guardadas correctamente.", "OK");
                    await Shell.Current.GoToAsync("//Inicio");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Hubo un problema al guardar.", "OK");
                }
            }
        }

        private void ActualizarTextoBoton()
        {
            TextoBoton = EsUltimaPregunta() ? "Enviar" : "Siguiente";
        }

        private void OnPropertyChanged([CallerMemberName] string? propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
