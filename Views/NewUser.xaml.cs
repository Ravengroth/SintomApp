using System.ComponentModel;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SintomApp.Class;
using SintomApp.Views;

namespace SintomApp.Views
{
    public enum RolIniciador
    {
        Admin,
        Medico,
        Paciente
    }

    public partial class NewUser : ContentPage, INotifyPropertyChanged
    {
        private RolIniciador rolIniciador;
        private bool esEdicion;
        private Usuario? _admin;
        private Usuario? _usuario;
        public NewUser()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);//Ocultamos la barra de navegación (1)
            this.BackgroundColor = Colors.Transparent;
            BindingContext = this;

            CargarUsuario();
            Title.Text = SessionManager.TituloPagina;
            esEdicion = SessionManager.EsEdicion;
            _usuario = SessionManager.UsuarioSeleccionado;
            _admin = SessionManager.UsuarioActual;
            rolIniciador = _admin.Admin == 0 ? RolIniciador.Paciente :
                           _admin.Admin == 1 ? RolIniciador.Admin :
                           RolIniciador.Medico;
            AjustarCheckBoxesSegunRol();
            if (esEdicion && _usuario != null) 
            {
                RellenarCampos(_usuario);

            }
        }

        public async void CargarUsuario()
        {
            if (SessionManager.UsuarioActual != null)
            {
                _admin = SessionManager.UsuarioActual;
                
            }
            else
            {
                // Fallback si se intenta entrar a la vista sin sesión
                await DisplayAlert("Error", "Sesión no iniciada. Vuelve a iniciar sesión.", "OK");
                await Navigation.PopToRootAsync();
            }
        }

        private readonly List<string> patologias = new()
        {
            "LES",
            "Miositis",
            "Esclerodermia",
            "Vasculitis"
        };

        private void RellenarCampos(Usuario _usuario)
        {
            TxtName.Text = _usuario.Nombre;
            TxtApellidos.Text = _usuario.Apellidos;
            TxtCorreo.Text = _usuario.Correo;
            TxtPhone.Text = _usuario.Telefono.ToString();
            TxtPass.Text = _usuario.Contrasena;
            //Title.Text = esEdicion ? "EDITAR USUARIO" : "NUEVO USUARIO";
            AddBtn.Text = esEdicion ? "EDITAR" : "AÑADIR";
            if (_usuario.Nacimiento != null)
            {
                Edad.Date = _usuario.Nacimiento.Value.ToDateTime(new TimeOnly(0, 0));
            }

            switch (_usuario.Admin)
            {
                case 0:
                    UsuarioCheckBox.IsChecked = true;
                    if(SessionManager.TituloPagina != "Editar perfil")
                    CamposExtraPaciente.IsVisible = true;
                    else CamposExtraPaciente.IsVisible = false;
                    Edad.Date = _usuario.Nacimiento?.ToDateTime(new TimeOnly(0, 0)) ?? DateTime.Today;
                    if (_usuario.Patologia != null)
                    {
                        Enfermedad.SelectedItem = patologias
                        .FirstOrDefault(p => p.Equals(_usuario.Patologia.Trim(), StringComparison.OrdinalIgnoreCase));
                        PickerMedicos.SelectedItem = _usuario.Medico;
                        // Asignar la frecuencia
                        TxtFrecuencia.Text = _usuario.Frecuencia.ToString();
                    }
                    break;
                case 1:
                    AdminCheckBox.IsChecked = true;
                    CamposExtraPaciente.IsVisible = false;
                    break;
                case 2:
                    MedicoCheckBox.IsChecked = true;
                    CamposExtraPaciente.IsVisible = false;
                    break;
                default:
                    break;

            }
        }

        private void AjustarCheckBoxesSegunRol()
        {
            switch (rolIniciador)
            {
                case RolIniciador.Paciente:
                    // Paciente solo puede registrar usuario, no muestra checkboxes
                    UsuarioCheckBox.IsChecked = true;
                    UsuarioCheckBox.IsVisible = false;
                    MedicoCheckBox.IsVisible = false;
                    AdminCheckBox.IsVisible = false;
                    MostrarCamposPaciente(false); // Siempre muestra los campos para paciente
                    break;

                case RolIniciador.Medico:
                    // Médico solo puede añadir usuarios, igual que paciente
                    UsuarioCheckBox.IsChecked = true;
                    UsuarioCheckBox.IsVisible = false;
                    MedicoCheckBox.IsVisible = false;
                    AdminCheckBox.IsVisible = false;

                    MostrarCamposPaciente(false);
                    break;

                case RolIniciador.Admin:
                    // Admin ve todos los checkbox para elegir rol
                    UsuarioCheckBox.IsVisible = true;
                    MedicoCheckBox.IsVisible = true;
                    AdminCheckBox.IsVisible = true;
                    UsuarioCheckBox.IsChecked = false; // por defecto
                    MostrarCamposPaciente(true);
                    break;
            }
        }


        private void MostrarCamposPaciente(bool mostrar)
        {
            CheckBoxes.IsVisible = mostrar;
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
            if (string.IsNullOrWhiteSpace(TxtCorreo.Text))
            {
                await BackgroundColorService.CambiarColorEntry(TxtCorreo, "#FFD6D6");
                contador++;
                return false;
            }
            if (string.IsNullOrWhiteSpace(TxtPhone.Text))
            {
                await BackgroundColorService.CambiarColorEntry(TxtPhone, "#FFD6D6");
                contador++;
                return false;
            }
            if (Enfermedad.SelectedItem == null && UsuarioCheckBox.IsChecked == true && SessionManager.TituloPagina != "Editar perfil")
            {
                //await BackgroundColorService.CambiarColorEntry(Enfermedad, "#FFD6D6");
                contador++;
                return false;
            }
            if(PickerMedicos.SelectedItem == null && UsuarioCheckBox.IsChecked == true && SessionManager.TituloPagina != "Editar perfil")
            {
                //await BackgroundColorService.CambiarColorEntry(PickerMedicos, "#FFD6D6");
                contador++;
                return false;
            }
            if (string.IsNullOrWhiteSpace(TxtFrecuencia.Text) && UsuarioCheckBox.IsChecked == true && SessionManager.TituloPagina != "Editar perfil")
            {
                await BackgroundColorService.CambiarColorEntry(TxtFrecuencia, "#FFD6D6");
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

        private async void Usuario_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                AdminCheckBox.IsChecked = false;
                MedicoCheckBox.IsChecked = false;

                MostrarCamposPaciente(true); 
                CamposExtraPaciente.IsVisible = true;
                await CargarPickerDeMedicos();
                Enfermedad.ItemsSource = patologias;
            }
        }

        private void Admin_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                UsuarioCheckBox.IsChecked = false;
                MedicoCheckBox.IsChecked = false;
                CamposExtraPaciente.IsVisible = false;

                MostrarCamposPaciente(true);
            }

        }
        private void Medico_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                UsuarioCheckBox.IsChecked = false;
                AdminCheckBox.IsChecked = false;
                CamposExtraPaciente.IsVisible = false;

                MostrarCamposPaciente(true);
            }
        }


        private async void DevolverUsuario()
        {
            // Validar correo
            if (!EsCorreoValido(TxtCorreo.Text))
            {
                await DisplayAlert("Error", "El correo no tiene un formato válido", "OK");
                return;
            }

            // Determinar rol seleccionado
            int rolSeleccionado = UsuarioCheckBox.IsChecked ? 0 :
                                  AdminCheckBox.IsChecked ? 1 :
                                  MedicoCheckBox.IsChecked ? 2 : -1;

            if (rolSeleccionado == -1)
            {
                await DisplayAlert("Error", "Debes seleccionar un rol", "OK");
                return;
            }

            // Validar teléfono (opcional, evita excepción al parsear)
            if (!int.TryParse(TxtPhone.Text, out int telefono))
            {
                await DisplayAlert("Error", "El teléfono no es válido", "OK");
                return;
            }

            // Crear o actualizar usuarioActual con datos del formulario
            if (rolSeleccionado == 0 )
            {
                if(SessionManager.TituloPagina == "Editar perfil")
                {
                    _usuario.Patologia = SessionManager.UsuarioActual.Patologia;
                    _usuario.Frecuencia = SessionManager.UsuarioActual.Frecuencia;
                    _usuario.PrimerDia = SessionManager.UsuarioActual.PrimerDia;
                    _usuario.Medico = SessionManager.UsuarioActual.Medico;
                }
                else
                {
                    // Paciente
                    _usuario.Patologia = Enfermedad.SelectedItem.ToString() ?? string.Empty;
                    _usuario.Frecuencia = int.Parse(TxtFrecuencia.Text);
                    _usuario.PrimerDia = DateTime.Today;
                    if (PickerMedicos?.SelectedItem is Usuario medicoSeleccionado)
                    {
                        int idMedico = medicoSeleccionado.Id;
                        // Guarda el ID donde lo necesites, por ejemplo:
                        _usuario.Medico = idMedico;
                    }
                }
                    
            }
            _usuario.Nombre = TxtName.Text;
            _usuario.Apellidos = TxtApellidos.Text;
            _usuario.Correo = TxtCorreo.Text;
            _usuario.Contrasena = TxtPass.Text;
            _usuario.Telefono = telefono;
            _usuario.Admin = rolSeleccionado;
            _usuario.Enable = true;
            DateTime fechaNacimiento = Edad.Date;
            DateOnly nacimineto = DateOnly.FromDateTime(fechaNacimiento);
            _usuario.Nacimiento = nacimineto;
            DateTime hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Year;

            // Corregir si aún no ha cumplido años este año
            if (fechaNacimiento.Date > hoy.AddYears(-edad))
            {
                edad--;
            }

            _usuario.Edad = edad;



            DatabaseManager dbManager = new DatabaseManager();

            bool resultado;

            if (esEdicion)
            {
                // Actualizar usuario existente
                resultado = await dbManager.ActualizarUsuarioAsync(_usuario);
            }
            else
            {
                // Insertar nuevo usuario
                resultado = await dbManager.InsertUsuarioAsync(_usuario);
            }

            if (resultado)
            {
                string tipo = rolSeleccionado == 0 ? "Usuario" : (rolSeleccionado == 1 ? "Administrador" : "Médico");
                string mensaje = esEdicion ? $"{tipo} actualizado correctamente" : $"{tipo} creado correctamente";
                await DisplayAlert("Éxito", mensaje, "OK");
                if (SessionManager.UsuarioActual.Admin > 0)
                    await Shell.Current.GoToAsync("//Admin");
                else await Shell.Current.GoToAsync("//Inicio");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo guardar el usuario", "OK");
            }
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

            PickerMedicos.ItemsSource = listaMedicos;
            //PickerMedicos.ItemDisplayBinding = new Binding("Nombre"); // O cualquier propiedad que represente visualmente al médico
        }

        private void PickerMedicos_SelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            if (picker?.SelectedItem is Usuario medicoSeleccionado)
            {
                int idMedico = medicoSeleccionado.Id;
                _usuario.Medico = idMedico;
            }
        }

    }

}

//Comprobar el correo sigue dando problemas
//Falta agregar comentarios en Admin.xaml.cs y sus derivados