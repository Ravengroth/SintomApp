using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Syncfusion.Maui.Charts;
using SintomApp.Class;

namespace SintomApp.Views
{
    public partial class UserData : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler? PropertyChanged;

        private Usuario _usuario;

        public UserData()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            this.BackgroundColor = Colors.Transparent;
            BindingContext = this;

            _usuario = SessionManager.UsuarioSeleccionado;
            TituloLabel.Text = $"Usuario: {_usuario.Nombre}";
        }

        private async void RegPreguntaBtnClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("RegPreguntas");
        }

        private async void PuntuacionesBtnClicked(object sender, EventArgs e)
        {
            await CargarGraficoPuntuaciones();
        }

        private async void CuestionariosClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("Cuestionarios");
        }

        private async void CalendarBtnClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("UserCalendar");
        }

        private async Task CargarGraficoPuntuaciones()
        {
            var dbManager = new DatabaseManager();
            var resultados = await dbManager.ObtenerPuntuacionesUsuario(_usuario);

            if (resultados == null || resultados.Count == 0)
            {
                await DisplayAlert("Sin datos", "No hay puntuaciones disponibles.", "OK");
                return;
            }

            var puntos = new ObservableCollection<PuntuacionChart>();

            foreach (var (fecha, puntuacion) in resultados.Take(10).Reverse()) // De más antigua a más reciente
            {
                puntos.Add(new PuntuacionChart
                {
                    Fecha = fecha.ToString("dd/MM"),
                    Puntuacion = puntuacion
                });
            }

            var serie = new ColumnSeries
            {
                ItemsSource = puntos,
                XBindingPath = nameof(PuntuacionChart.Fecha),
                YBindingPath = nameof(PuntuacionChart.Puntuacion),
                DataLabelSettings = new CartesianDataLabelSettings
                {
                    UseSeriesPalette = true,
                    LabelPlacement = DataLabelPlacement.Outer
                }
            };

            ScoreChart.Series.Clear();
            ScoreChart.Series.Add(serie);

            // Mostrar media si quieres
            decimal media = resultados.Take(10).Average(r => r.puntuacion);
            MediaLabel.Text = $"Media: {media:F2}";
        }



        public class PuntuacionChart
        {
            public string Fecha { get; set; }
            public decimal Puntuacion { get; set; }
        }
    }
}
