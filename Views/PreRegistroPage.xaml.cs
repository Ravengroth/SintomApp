using SintomApp.Class;
using System.Text.RegularExpressions;

namespace SintomApp.Views
{
    public partial class PreRegistroPage : ContentPage
    {
        public PreRegistroPage()
        {
            InitializeComponent();
        }

        private async void OnVerificarClicked(object sender, EventArgs e)
        {
            string correo = CorreoEntry.Text?.Trim();
            string token = TokenEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(token))
            {
                await DisplayAlert("Error", "Por favor, rellene ambos campos.", "OK");
                return;
            }

            if (!EsCorreoValido(correo))
            {
                await DisplayAlert("Error", "El formato del correo es inválido.", "OK");
                return;
            }

            var db = new DatabaseManager();
            var usuario = await db.ObtenerUsuarioPorCorreoAsync(correo, token); // Debes implementar esto
            DateTime caducidad = usuario.Fecha_generacion.Value.AddDays(15); // Asumiendo que la fecha de generación es un campo DateTime en el usuario
            
            // Verificar si el usuario es nulo o si el token ha caducado
            if (usuario == null)
            {
                await DisplayAlert("Error", "El correo o el token no son válidos o no están autorizados.", "OK");
                return;
            }
            
            if (caducidad < DateTime.Now.AddMinutes(5))
            {
                await DisplayAlert("Error", "El token ha caducado o no es válido.", "OK");
                return;
            }
            else if(usuario.Usado == true)
            {
                await DisplayAlert("Error", "El token ya ha sido usado, compruebe que no esta registrado o consultelo con el médico.", "OK");
                return;
            }
            else
            {
                await DisplayAlert("Correcto", "Token verificado correctamente.", "OK");
            }

            // Si pasa la verificación, navegar a UserRegister y pasar datos
            SessionManager.UsuarioActual = usuario; // Asignar el usuario preasignado a la sesión
            await Shell.Current.GoToAsync("UserRegister");
        }

        private bool EsCorreoValido(string correo)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(correo, pattern);
        }
    }
}
