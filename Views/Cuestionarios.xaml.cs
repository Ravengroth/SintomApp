using System.ComponentModel;
using SintomApp.Class;
using MySqlConnector;

namespace SintomApp.Views
{
    public partial class Cuestionarios : ContentPage, INotifyPropertyChanged
    {
        public Usuario _usuario;
        private List<CuestionarioRespondido> _cuestionarios = new();

        public new event PropertyChangedEventHandler? PropertyChanged;

        public Cuestionarios()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            BackgroundColor = Colors.Transparent;
            BindingContext = this;
            _usuario = SessionManager.UsuarioSeleccionado;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            DatabaseManager dbManager = new DatabaseManager();

            var resultados = await dbManager.ObtenerPuntuacionesUsuario(_usuario);

            _cuestionarios = resultados
                .OrderByDescending(r => r.fecha)
                .Select(r => new CuestionarioRespondido
                {
                    Fecha = r.fecha,
                    Puntuacion = r.puntuacion
                }).ToList();

            QuestView.ItemsSource = _cuestionarios;
        }

        private void FiltroPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            string? filtro = FiltroPicker.SelectedItem?.ToString() ?? "Fecha";
            List<CuestionarioRespondido> filtrados = filtro switch
            {
                "Puntuación" => _cuestionarios.OrderByDescending(q => q.Puntuacion).ToList(),
                _ => _cuestionarios.OrderByDescending(q => q.Fecha).ToList()
            };

            QuestView.ItemsSource = filtrados;
        }

        private async void LoadQuestBtnClicked(object sender, EventArgs e)
        {
            var boton = sender as Button;
            var cuestionario = boton?.BindingContext as CuestionarioRespondido;

            if (cuestionario != null)
            {
                SessionManager.UsuarioSeleccionado = _usuario;
                SessionManager.FechaSeleccionada = cuestionario.Fecha;
                await Shell.Current.GoToAsync(nameof(RespuestasPorFecha));

            }
        }
        private async void VovlerClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Confirmar", "Pulse <Si> para volver.", "Sí", "No");
            if (confirmar)
            {
                await Shell.Current.GoToAsync("//Admin");
            }
        }
    }
}
