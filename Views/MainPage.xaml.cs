using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SintomApp.Class;
using SintomApp.Views;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;


namespace SintomApp.Views
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {

        public new event PropertyChangedEventHandler? PropertyChanged;
        public class ResultadoLogin
        {
            public string Estado { get; set; }
            public string? Token { get; set; }
            public string? Mensaje { get; set; }
        }

        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);//Ocultamos la barra de navegación (1)
            this.BackgroundColor = Colors.Transparent;
            BindingContext = this;
            SessionManager.UsuarioActual = null; // Aseguramos que no haya usuario activo al iniciar

            string original = "miClave123";
            byte[] cifrado = RSAHelper.EncryptToBytes(original);
            string descifrado = RSAHelper.DecryptFromBytes(cifrado);

            Console.WriteLine(descifrado == original ? "Correcto" : "Incorrecto");

        }

        private void LogBtnClicked(object sender, EventArgs e)
        {
            ValidarUsuario(TxtName.Text, TxtPass.Text);
        }

        private async void ValidarUsuario(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Validez.Text = "Por favor, rellene ambos campos";
                return;
            }
            var dbManager = new DatabaseManager();
            Usuario usuario = null;
            var estado = await dbManager.ComprobarCredenciales(username, password);
            switch (estado)
            {
                case DatabaseManager.EstadoCredencial.UsuarioNoExiste:
                    Validez.Text = "Usuario no existe";
                    await BackgroundColorService.CambiarColorLog(TxtName, TxtPass, "#FFD6D6");  //Rojo
                    break;
                case DatabaseManager.EstadoCredencial.UsuarioDeshabilitado:
                    Validez.Text = "Usuario o contraseña incorrectos";
                    await BackgroundColorService.CambiarColorLog(TxtName, TxtPass, "#FFD6D6");
                    break;
                case DatabaseManager.EstadoCredencial.ContrasenaIncorrecta:
                    Validez.Text = "Contraseña incorrecta";
                    await BackgroundColorService.CambiarColorEntry(TxtPass, "#FFD6D6");
                    break;
                case DatabaseManager.EstadoCredencial.Autenticado:
                case DatabaseManager.EstadoCredencial.Admin:
                case DatabaseManager.EstadoCredencial.Medico:
                    usuario = await dbManager.CargarUsuarioBD(username, password);
                    await RedirigirPorRol(estado, usuario);
                    break;
                case DatabaseManager.EstadoCredencial.ErrorConexion:
                    await DisplayAlert("Error", "No se pudo conectar con la base de datos", "OK");
                    Validez.Text = "Error de conexión";
                    break;
            }

        }

        private async Task RedirigirPorRol(DatabaseManager.EstadoCredencial estado, Usuario usuario)
        {
            SessionManager.UsuarioActual = usuario;
            switch (estado)
            {
                case DatabaseManager.EstadoCredencial.Autenticado:
                    await Shell.Current.GoToAsync("//Inicio");
                    break;

                case DatabaseManager.EstadoCredencial.Admin:
                    await Shell.Current.GoToAsync("//Admin");
                    break;

                case DatabaseManager.EstadoCredencial.Medico:
                    await Shell.Current.GoToAsync("//Admin");
                    break;
            }

        }



        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ViewPassCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            TxtPass.IsPassword = !e.Value;

        }

        public ICommand TapCommand => new Command<string>(async (param) =>
        {
            if (param == "Registrar")
            {
                await Navigation.PushAsync(new PreRegistroPage());

            }
        });


    }

}


/*(1) Ocultar la barra de navegación en la página principal 
 * En Android se modifica el archivo MainActivity.cs
 * En iOS se podifica el archivo AppDelegate.s*/