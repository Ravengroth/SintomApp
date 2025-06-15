using System.ComponentModel;
using System.Runtime.CompilerServices;
using SintomApp.Class;
using SintomApp.Views;

namespace SintomApp.Views
{
    public partial class DelUser : ContentPage, INotifyPropertyChanged
    {
        List<Usuario> usuarios = new List<Usuario>();
        public Usuario? _usuario;

        public new event PropertyChangedEventHandler? PropertyChanged;
        public DelUser()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);//Ocultamos la barra de navegación (1)
            this.BackgroundColor = Colors.Transparent;
            BindingContext = this;
            CargarUsuario();
        }
        public async void CargarUsuario()
        {
           
            _usuario = SessionManager.UsuarioActual;
            
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            DatabaseManager dbManager = new DatabaseManager();
            usuarios = await dbManager.GetUsuariosAsync(_usuario.Id,_usuario.Admin);
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

        private async void DelUserBtnClicked(object sender, EventArgs e)
        {
            DatabaseManager dbManager = new DatabaseManager();
            var boton = sender as Button;
            var usuario = boton?.BindingContext as Usuario;

            if (usuario != null)
            {
                bool eliminado = await dbManager.DeshabilitarUsuario(usuario);
                if(eliminado)
                {
                    await DisplayAlert("Usuario eliminado", "El usuario ha sido eliminado correctamente.", "OK");
                    usuarios.Remove(usuario);
                    UsuariosView.ItemsSource = null;
                    UsuariosView.ItemsSource = usuarios;

                    await Shell.Current.GoToAsync("//Admin");

                    //await Navigation.PushAsync(new Admin());
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo eliminar el usuario.", "OK");
                }
            }
        }



    }

}