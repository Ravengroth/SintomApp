using System.ComponentModel;
using System.Runtime.CompilerServices;
using SintomApp.Class;
using SintomApp.Views;
using MySqlConnector;

namespace SintomApp.Views
{
    public partial class LoadUser : ContentPage, INotifyPropertyChanged
    {
        List<Usuario> usuarios = new List<Usuario>();
        public Usuario? _usuario;

        public new event PropertyChangedEventHandler? PropertyChanged;
        public LoadUser()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);//Ocultamos la barra de navegación (1)
            this.BackgroundColor = Colors.Transparent;
            BindingContext = this;
            CargarUsuario();
        }

        public async void CargarUsuario()
        {
            if (SessionManager.UsuarioActual != null)
            {
                _usuario = SessionManager.UsuarioActual;
            }
            else
            {
                // Fallback si se intenta entrar a la vista sin sesión
                await DisplayAlert("Error", "Sesión no iniciada. Vuelve a iniciar sesión.", "OK");
                await Navigation.PopToRootAsync();
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            DatabaseManager dbManager = new DatabaseManager();
            usuarios = await dbManager.GetUsuariosAsync(_usuario.Id, _usuario.Admin);
            UsuariosView.ItemsSource = usuarios;

        }
        private void FiltroPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            string? filtro = FiltroPicker.SelectedItem?.ToString() ?? "Nombre";

            List<Usuario> usuariosFiltrados = filtro switch
            {
                "Nombre" => usuarios.OrderBy(u => u.Nombre).ToList(),
                "Edad" => usuarios.OrderBy(u => u.Telefono).ToList(),
                "Fecha de creación" => usuarios.OrderBy(u => u.FechaCreacion).ToList(),
                "Patología" => usuarios.OrderBy(u => u.Patologia).ToList(),
                _ => usuarios.OrderBy(u => u.Nombre).ToList()
            };
            UsuariosView.ItemsSource = usuariosFiltrados;
        }

        private async void LoadUserBtnClicked(object sender, EventArgs e)
        {
            var boton = sender as Button;
            var usuario = boton?.BindingContext as Usuario;

            if (usuario != null)
            {
                SessionManager.UsuarioSeleccionado = usuario; // Guardamos el usuario seleccionado en la sesión
                await Shell.Current.GoToAsync(nameof(UserData));
            }
        }

    }

}