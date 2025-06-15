using System.ComponentModel;
using System.Runtime.CompilerServices;
using SintomApp.Class;
using SintomApp.Views;

namespace SintomApp.Views
{
    public partial class EditUser : ContentPage, INotifyPropertyChanged
    {
        List<Usuario> usuarios = new List<Usuario>();

        public new event PropertyChangedEventHandler? PropertyChanged;
        private Usuario? _admin;
        public EditUser()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);//Ocultamos la barra de navegación (1)
            this.BackgroundColor = Colors.Transparent;
            BindingContext = this;
            CargarUsuario();
            
        }
        public async void CargarUsuario()
        {
                _admin = SessionManager.UsuarioActual;
                FiltroPicker.ItemsSource = new List<string>
                {
                    "Nombre",
                    "Apellidos",
                    "Fecha de creación",
                    "Patología: LES",
                    "Patología: Vasculitis",
                    "Patología: Esclerodermia",
                    "Patología: Miositis",
                    "Solo habilitados",
                    "Solo deshabilitados"
                };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            DatabaseManager dbManager = new DatabaseManager();
            usuarios = await dbManager.GetUsuariosAsync(_admin.Id, _admin.Admin);
            UsuariosView.ItemsSource = usuarios;
        }

        private void BusquedaUsuarios_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarUsuarios();
        }

        private void BusquedaUsuarios_SearchButtonPressed(object sender, EventArgs e)
        {
            FiltrarUsuarios();
        }

        private void FiltroPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            FiltrarUsuarios();
        }

        private void FiltrarUsuarios()
        {

            string textoBusqueda = BusquedaUsuarios.Text?.ToLower() ?? "";
            string? filtroSeleccionado = FiltroPicker.SelectedItem?.ToString() ?? "Nombre";

            var usuariosFiltrados = usuarios.AsEnumerable();

            // Primero aplica el filtro principal (por patología, habilitados, etc.)
            usuariosFiltrados = filtroSeleccionado switch
            {
                "Apellidos" => usuariosFiltrados.OrderBy(u => u.Apellidos),
                "Fecha de creación" => usuariosFiltrados.OrderBy(u => u.FechaCreacion),
                "Patología: LES" => usuariosFiltrados.Where(u => u.Patologia == "LES").OrderBy(u => u.Nombre),
                "Patología: Vasculitis" => usuariosFiltrados.Where(u => u.Patologia == "Vasculitis").OrderBy(u => u.Nombre),
                "Patología: Esclerodermia" => usuariosFiltrados.Where(u => u.Patologia == "Esclerodermia").OrderBy(u => u.Nombre),
                "Patología: Miositis" => usuariosFiltrados.Where(u => u.Patologia == "Miositis").OrderBy(u => u.Nombre),
                "Solo habilitados" => usuariosFiltrados.Where(u => u.Enable == true).OrderBy(u => u.Nombre),
                "Solo deshabilitados" => usuariosFiltrados.Where(u => u.Enable == false).OrderBy(u => u.Nombre),
                _ => usuariosFiltrados.OrderBy(u => u.Nombre)
            };

            // Después aplica la búsqueda por texto
            if (!string.IsNullOrEmpty(textoBusqueda))
            {
                usuariosFiltrados = usuariosFiltrados.Where(u =>
                    (u.Nombre?.ToLower().Contains(textoBusqueda) ?? false) ||
                    (u.Apellidos?.ToLower().Contains(textoBusqueda) ?? false) ||
                    (u.Correo?.ToLower().Contains(textoBusqueda) ?? false)
                );
            }

            UsuariosView.ItemsSource = usuariosFiltrados.ToList();
        }


        private async void EditUserBtnClicked(object sender, EventArgs e)
        {
            var boton = sender as Button;
            var usuario = boton?.BindingContext as Usuario;

            if (usuario != null)
            {
                SessionManager.UsuarioSeleccionado = usuario; // Guardamos el usuario seleccionado en la sesión
                SessionManager.EsEdicion = true; // Indicamos que estamos en modo edición
                SessionManager.TituloPagina = "EDITAR USUARIO"; // Título de la edición
                await Shell.Current.GoToAsync("NewUser");
            }
        }
       


    }

}