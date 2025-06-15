using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SintomApp.Class;
using SintomApp.Views;

namespace SintomApp.Views
{
    public partial class UserRegister : ContentPage, INotifyPropertyChanged
    {
        private bool esEdicion;
        private Usuario _usuarioActual;
        public UserRegister()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);//Ocultamos la barra de navegación (1)
            this.BackgroundColor = Colors.Transparent;
            BindingContext = this;
            _usuarioActual = SessionManager.UsuarioActual ?? new Usuario();
            RellenarCampos();
        }
        private readonly List<string> patologias = new()
        {
            "LES",
            "Miositis",
            "Esclerodermia",
            "Vasculitis"
        };
        public async void RellenarCampos()
        {
            if (_usuarioActual != null)
            {
                TxtName.Text = _usuarioActual.Nombre;
                TxtPass.Text = _usuarioActual.Token;
                TxtApellidos.Text = _usuarioActual.Apellidos;
                TxtCorreo.Text = _usuarioActual.Correo;
                TxtPhone.Text = _usuarioActual.Telefono.ToString();
                PatologiaPicker.ItemsSource = patologias;
                await CargarPickerDeMedicos();
            }
            else
            {
                await DisplayAlert("Error", "No se ha podido cargar el usuario actual.", "OK");
            }
        }

        private async void AddBtnClicked(object sender, EventArgs e)
        {
            bool respuesta = await ComprobarCampos();

            if (!respuesta) return;
            DevolverUsuario();
        }

        private async Task<bool> ComprobarCampos()
        {
            int contador = 0;
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                await BackgroundColorService.CambiarColorEntry(TxtName, "#FFD6D6");
                contador++;
                return false;
            }
            if (string.IsNullOrWhiteSpace(TxtPass.Text))
            {
                await BackgroundColorService.CambiarColorEntry(TxtPass, "#FFD6D6");
                contador++;
                return false;
            }
            if (string.IsNullOrWhiteSpace(TxtApellidos.Text))
            {
                await BackgroundColorService.CambiarColorEntry(TxtApellidos, "#FFD6D6");
                contador++;
                return false;
            }
            if (string.IsNullOrWhiteSpace(TxtCorreo.Text))
            {
                await BackgroundColorService.CambiarColorEntry(TxtCorreo, "#FFD6D6");
                contador++;
                return false;
            }
            if (contador == 0)
            {

                bool respuesta = await DisplayAlert("Confirmación", "¿Está segura de que quiere continuar?", "Aceptar", "Cancelar");
                return respuesta;
            }

            return false;
        }


        private async void DevolverUsuario()
        {
            if (!EsCorreoValido(TxtCorreo.Text))
            {
                await DisplayAlert("Error", "El correo no tiene un formato válido", "OK");
                return;
            }
            _usuarioActual.Nombre = TxtName.Text;
            _usuarioActual.Contrasena = TxtPass.Text;
            _usuarioActual.Apellidos = TxtApellidos.Text;
            _usuarioActual.Correo = TxtCorreo.Text;
            if (int.TryParse(TxtPhone.Text, out int telefono))
            {
                _usuarioActual.Telefono = telefono;
            }
            else
            {
                await DisplayAlert("Error", "El número de teléfono no es válido.", "OK");
                return;
            }
            _usuarioActual.FechaCreacion = DateTime.Now;
            _usuarioActual.Nacimiento = DateOnly.FromDateTime(TxtFechaNac.Date);
            _usuarioActual.Medico = MedicoPicker.SelectedItem is Usuario medicoSeleccionado ? medicoSeleccionado.Id : 0;
            _usuarioActual.Edad = CalcularEdad(TxtFechaNac.Date);
            _usuarioActual.Patologia = PatologiaPicker.SelectedItem?.ToString() ?? "No especificada";
            var db = new DatabaseManager();
            bool resultado = await db.InsertUsuarioAsync(_usuarioActual);
            if (!resultado)
            {
                await DisplayAlert("Error", "No se pudo registrar el usuario. Inténtelo de nuevo.", "OK");
                return;
            }


            await DisplayAlert("Correcto", "Usuario validado correctamente.", "OK");
            // Continuar con flujo de registro
            await Shell.Current.GoToAsync("//MainPage");
        }

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }


        private void TxtCorreo_TextChanged(object sender, TextChangedEventArgs e)
        {
            var email = e.NewTextValue;
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(email, pattern))
            {
                // Opcional: Puedes mostrar un mensaje o cambiar el borde del Entry
                TxtCorreo.TextColor = Colors.Red;
            }
            else
            {
                TxtCorreo.TextColor = Colors.Black;
            }
        }
        private bool EsCorreoValido(string correo)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(correo, pattern);
        }

        private void ViewPassCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            TxtPass.IsPassword = !e.Value;
        }
        private async Task CargarPickerDeMedicos()
        {
            DatabaseManager dbManager = new DatabaseManager();
            var listaMedicos = await dbManager.ObtenerMedicosAsync();

            MedicoPicker.ItemsSource = listaMedicos;
            //PickerMedicos.ItemDisplayBinding = new Binding("Nombre"); // O cualquier propiedad que represente visualmente al médico
        }

        private void PickerMedicos_SelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            if (picker?.SelectedItem is Usuario medicoSeleccionado)
            {
                int idMedico = medicoSeleccionado.Id;
                // Guarda el ID donde lo necesites, por ejemplo:
                _usuarioActual.Medico = idMedico;
            }
        }
    }

}