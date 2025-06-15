using System.ComponentModel;
using System.Runtime.CompilerServices;
using SintomApp.Class;
using SintomApp.Views;
using MySqlConnector;

namespace SintomApp.Views
{
    public partial class RegPreguntas : ContentPage, INotifyPropertyChanged
    {
        List<Pregunta> preguntas = new List<Pregunta>();
        public Usuario _usuario;

        public new event PropertyChangedEventHandler? PropertyChanged;
        public RegPreguntas()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);//Ocultamos la barra de navegación (1)
            this.BackgroundColor = Colors.Transparent;
            BindingContext = this;
            _usuario = SessionManager.UsuarioSeleccionado;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            DatabaseManager dbManager = new DatabaseManager();
            preguntas = await dbManager.ObtenerPreguntasPorId(_usuario);
            QuestView.ItemsSource = preguntas;

        }
        private void FiltroPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            string? filtro = FiltroPicker.SelectedItem?.ToString() ?? "Nombre";

            List<Pregunta> preguntasFiltradas = filtro switch
            {
                "Id" => preguntas.OrderBy(u => u.Id).ToList(),
                "Puntuación" => preguntas.OrderBy(u => u.Puntuacion).ToList(),
                _ => preguntas.OrderBy(u => u.Texto).ToList()
            };
            QuestView.ItemsSource = preguntasFiltradas;
        }

        private async void LoadQuestBtnClicked(object sender, EventArgs e)
        {
            var boton = sender as Button;
            var pregunta = boton?.BindingContext as Pregunta;

            if (pregunta != null)
            {
                // Navegar a la página de respuestas para la pregunta seleccionada
                SessionManager.PreguntaSeleccionada = pregunta;
                SessionManager.UsuarioSeleccionado = _usuario;
                await Shell.Current.GoToAsync(nameof(RespuestasPregunta));
            }
        }

    }

}