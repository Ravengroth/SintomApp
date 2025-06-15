using System.Globalization;
using SintomApp.Class;

namespace SintomApp.Views
{
    public partial class UserCalendar : ContentPage
    {
        DateTime fechaActual;
        DateTime fechaSeleccionadaActual;
        public Usuario usuarioActual;

        public UserCalendar()
        {
            InitializeComponent();
            fechaActual = DateTime.Today;
            if(SessionManager.UsuarioActual.Id > 0)
            {
                usuarioActual = SessionManager.UsuarioSeleccionado;
            }
            else
            {
                usuarioActual = SessionManager.UsuarioActual;
            }

                GenerarCalendario(fechaActual);

            // Botón para añadir evento
            var botonAgregarEvento = new Button
            {
                Text = "➕ Añadir Evento",
                BackgroundColor = Colors.LightBlue,
                CornerRadius = 15,
                Margin = new Thickness(10)
            };
            botonAgregarEvento.Clicked += (s, e) =>
            {
                EventoStack.IsVisible = true;
                EventoFecha.Date = fechaSeleccionadaActual != default ? fechaSeleccionadaActual : DateTime.Today;
            };
            MainStackLayout.Children.Add(botonAgregarEvento);
        }

        async void GenerarCalendario(DateTime fecha)
        {
            CalendarGrid.Children.Clear();
            CalendarGrid.ColumnDefinitions.Clear();
            CalendarGrid.RowDefinitions.Clear();

            var fechasEncuesta = ObtenerFechasDeEncuesta(usuarioActual.PrimerDia ?? DateTime.Today, (int)usuarioActual.Frecuencia, fecha);
            var dbManager = new DatabaseManager();
            var fechasRealizadas = await dbManager.ObtenerFechasEncuestasRealizadas(usuarioActual.Id);
            var eventosDelMes = await dbManager.ObtenerEventosDelMes(usuarioActual.Id, fecha.Year, fecha.Month); // nuevo

            for (int i = 0; i < 7; i++)
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            for (int i = 0; i < 7; i++)
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            MesLabel.Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(fecha.Month).ToUpper() + " " + fecha.Year;

            string[] dias = { "D", "L", "M", "X", "J", "V", "S" };
            for (int i = 0; i < 7; i++)
            {
                var label = new Label
                {
                    Text = dias[i],
                    HorizontalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, i);
                CalendarGrid.Children.Add(label);
            }

            DateTime primerDiaMes = new DateTime(fecha.Year, fecha.Month, 1);
            int diasEnMes = DateTime.DaysInMonth(fecha.Year, fecha.Month);
            int diaSemana = (int)primerDiaMes.DayOfWeek;

            int fila = 1;
            int columna = diaSemana;

            for (int dia = 1; dia <= diasEnMes; dia++)
            {
                DateTime fechaActualDia = new DateTime(fecha.Year, fecha.Month, dia);
                bool esDiaEncuesta = fechasEncuesta.Contains(fechaActualDia.Date);
                bool encuestaRealizada = fechasRealizadas.Contains(fechaActualDia.Date);
                bool esPrimerDia = usuarioActual.PrimerDia?.Date == fechaActualDia.Date;
                bool tieneEvento = eventosDelMes.Any(e => e.Fecha.Date == fechaActualDia.Date);

                Color colorFondo;

                if (encuestaRealizada)
                    colorFondo = Colors.LightGreen;
                else if (esDiaEncuesta && fechaActualDia < DateTime.Today)
                    colorFondo = Colors.Red;
                else if (esDiaEncuesta)
                    colorFondo = Colors.Orange;
                else if (tieneEvento)
                    colorFondo = Colors.LightBlue;
                else
                    colorFondo = Colors.White;

                var botonDia = new Button
                {
                    Text = dia.ToString(),
                    BackgroundColor = colorFondo,
                    TextColor = Colors.Black,
                    CornerRadius = 10
                };

                Grid.SetRow(botonDia, fila);
                Grid.SetColumn(botonDia, columna);
                CalendarGrid.Children.Add(botonDia);

                botonDia.Clicked += async (s, e) =>
                {
                    fechaSeleccionadaActual = fechaActualDia;
                    await ActualizarResumenDelDia(fechaActualDia);
                };

                columna++;
                if (columna > 6)
                {
                    columna = 0;
                    fila++;
                }
            }
        }

        async Task ActualizarResumenDelDia(DateTime diaSeleccionado)
        {
            var dbManager = new DatabaseManager();
            string resumen = $"📅 {diaSeleccionado:dd/MM/yyyy}:\n";

            var estado = await dbManager.EstadoEncuesta(usuarioActual.Id, diaSeleccionado);

            if (FechaEsEncuesta(usuarioActual.PrimerDia ?? DateTime.Today, (int)usuarioActual.Frecuencia, diaSeleccionado))
            {
                resumen += estado == true ? "✅ Encuesta ya realizada.\n"
                         : estado == false ? "📝 Encuesta pendiente.\n"
                         : "⚠ Error al comprobar encuesta.\n";
            }
            else
            {
                resumen += "No hay tareas asignadas.\n";
            }

            var eventosImportantes = await dbManager.ObtenerEventos(usuarioActual.Id, diaSeleccionado);
            foreach (var evento in eventosImportantes)
            {
                resumen += $"📌 {evento.Tipo}: {evento.Descripcion}\n";
            }

            ResumenLabel.Text = resumen;
        }

        private async void GuardarEvento_Clicked(object sender, EventArgs e)
        {
            DateTime fechaElegida = EventoFecha.Date;
            string tipo = EventoTipo.Text?.Trim();
            string descripcion = EventoDescripcion.Text?.Trim();
            string frecuenciaStr = EventoFrecuencia.Text?.Trim();

            if (fechaElegida < DateTime.Today)
            {
                await DisplayAlert("Error", "No puedes agregar eventos en fechas pasadas.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(descripcion))
            {
                await DisplayAlert("Error", "Por favor completa todos los campos.", "OK");
                return;
            }

            if (!int.TryParse(frecuenciaStr, out int frecuencia) || frecuencia < 0)
            {
                await DisplayAlert("Error", "Frecuencia inválida (usa 0 para evento único).", "OK");
                return;
            }

            var db = new DatabaseManager();
            Guid idGrupo = Guid.NewGuid(); // para vincular repeticiones

            var eventoInicial = new Evento
            {
                IdUsuario = usuarioActual.Id,
                Fecha = fechaElegida,
                Tipo = tipo,
                Descripcion = descripcion,
                IdGrupo = idGrupo.ToString()
            };

            await db.InsertarEvento(eventoInicial);

            if (frecuencia > 0)
            {
                DateTime siguiente = fechaElegida.AddDays(frecuencia);
                while (siguiente <= fechaActual.AddMonths(1))
                {
                    var eventoRepetido = new Evento
                    {
                        IdUsuario = usuarioActual.Id,
                        Fecha = siguiente,
                        Tipo = tipo,
                        Descripcion = descripcion + $" (repetido)",
                        IdGrupo = idGrupo.ToString()
                    };
                    await db.InsertarEvento(eventoRepetido);
                    siguiente = siguiente.AddDays(frecuencia);
                }
            }

            await DisplayAlert("Evento añadido", "El evento se ha guardado correctamente.", "OK");

            EventoTipo.Text = "";
            EventoDescripcion.Text = "";
            EventoFrecuencia.Text = "";
            EventoStack.IsVisible = false;

            GenerarCalendario(fechaActual); // refresca calendario
        }

        private Task<DateTime> DisplayDatePicker(string title)
        {
            var tcs = new TaskCompletionSource<DateTime>();
            var datePicker = new DatePicker
            {
                MinimumDate = DateTime.Today,
                MaximumDate = DateTime.Today.AddYears(5),
                Date = DateTime.Today
            };

            var aceptar = new Button { Text = "Aceptar" };
            aceptar.Clicked += (s, e) =>
            {
                tcs.TrySetResult(datePicker.Date);
                Navigation.PopModalAsync();
            };

            var page = new ContentPage
            {
                Content = new StackLayout
                {
                    Padding = 20,
                    VerticalOptions = LayoutOptions.Center,
                    Children = { new Label { Text = title }, datePicker, aceptar }
                }
            };

            Navigation.PushModalAsync(page);
            return tcs.Task;
        }

        bool FechaEsEncuesta(DateTime primerDia, int frecuenciaDias, DateTime fecha)
        {
            if (frecuenciaDias <= 0) return false;
            DateTime actual = primerDia;
            while (actual <= fecha)
            {
                if (actual.Date == fecha.Date) return true;
                actual = actual.AddDays(frecuenciaDias);
            }
            return false;
        }

        List<DateTime> ObtenerFechasDeEncuesta(DateTime primerDia, int frecuenciaDias, DateTime mesActual)
        {
            var fechas = new List<DateTime>();
            if (frecuenciaDias <= 0) return fechas;

            DateTime fecha = primerDia;
            DateTime limiteSuperior = new DateTime(mesActual.Year, mesActual.Month, 1).AddMonths(1);

            while (fecha < limiteSuperior)
            {
                if (fecha.Month == mesActual.Month && fecha.Year == mesActual.Year)
                    fechas.Add(fecha);

                fecha = fecha.AddDays(frecuenciaDias);
            }

            return fechas;
        }

        private void CancelarEvento_Clicked(object sender, EventArgs e)
        {
            EventoTipo.Text = "";
            EventoDescripcion.Text = "";
            EventoFrecuencia.Text = "";
            EventoStack.IsVisible = false;
        }

        private void MesAnterior_Clicked(object sender, EventArgs e)
        {
            fechaActual = fechaActual.AddMonths(-1);
            GenerarCalendario(fechaActual);
        }

        private void MesSiguiente_Clicked(object sender, EventArgs e)
        {
            fechaActual = fechaActual.AddMonths(1);
            GenerarCalendario(fechaActual);
        }
    }
}
