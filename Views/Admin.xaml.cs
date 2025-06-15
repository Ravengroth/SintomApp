using System.ComponentModel;
using System.Runtime.CompilerServices;
using SintomApp.Class;
using SintomApp.Views;

namespace SintomApp.Views
{
    public partial class Admin : ContentPage, INotifyPropertyChanged
    {
        

        public new event PropertyChangedEventHandler? PropertyChanged;
        public Usuario admin;
        public Admin()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);//Ocultamos la barra de navegación (1)
            this.BackgroundColor = Colors.Transparent;
            BindingContext = this;
            admin = SessionManager.UsuarioActual;
        }

        private async void NewUserBtnClicked(object sender, EventArgs e)
        {
            SessionManager.UsuarioSeleccionado = null; // Asegúrate de limpiar la selección actual antes de crear un nuevo usuario
            SessionManager.EsEdicion = false; // Asegúrate de que no estamos en modo edición
            SessionManager.TituloPagina = "AÑADIR USUARIO"; // Establece el título de la página
            await Shell.Current.GoToAsync("NewUser");
        }
        private async void EditUserBtnClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("EditUser");
        }
        private async void LoadUserBtnClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("LoadUser");
        }
        private async void DelUserBtnClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("DelUser");

        }
        private async void GenerarContrasenaBtnClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("GenerarToken");

        }
        private async void AvisosBtnClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("AvisosPage"); // Asegúrate de que este nombre esté registrado en tu Shell.
        }

        public async void LogoutBtnClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Confirmar", "¿Estás seguro de que deseas cerrar sesión?", "Sí", "No");
            if (confirmar)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
        }


    }

}