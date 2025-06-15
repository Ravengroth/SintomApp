using System.ComponentModel;
using System.Text.Json;
using SintomApp.Class;
using Microsoft.Win32;

namespace SintomApp.Views
{
    public partial class Inicio : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler? PropertyChanged;
        private Usuario usuarioInicio;
        public Inicio()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);//Ocultamos la barra de navegación (1)
            this.BackgroundColor = Colors.Transparent;
            BindingContext = this;

        }
        public async void CargarUsuario()
        {
            if (SessionManager.UsuarioActual != null)
            {
                //SessionManager.UsuarioActual = usuarioInicio; // Aseguramos que el usuario actual esté actualizado
                SessionManager.TituloPagina = "Editar perfil";
                SessionManager.EsEdicion = true;
                SessionManager.UsuarioSeleccionado = SessionManager.UsuarioActual; // Aseguramos que el usuario seleccionado sea el actual
                usuarioInicio = SessionManager.UsuarioActual;
                RellenarInicio(usuarioInicio);

            }
            else
            {
                // Fallback si se intenta entrar a la vista sin sesión
                await DisplayAlert("Error", "Sesión no iniciada. Vuelve a iniciar sesión.", "OK");
                await Navigation.PopToRootAsync();
            }
        }

        private async void RellenarInicio(Usuario _user)
        {
            switch (_user.Patologia)
            {
                case "LES": // Morado-rosado alegre, sin blanco
                    CargarVistaGenerica("LES", "mariposa.jpeg", Color.FromArgb("#D87ED8"), Color.FromArgb("#F672C1"));
                    break;
                case "Miositis": // Azul cielo y azul vivo
                    CargarVistaGenerica("Miositis", "reumatologia.png", Color.FromArgb("#4A90E2"), Color.FromArgb("#71B6F9"));
                    break;
                case "Esclerodermia": // Verdes claros y vivos
                    CargarVistaGenerica("Esclerodermia", "esclerodermia.jpg", Color.FromArgb("#7ED957"), Color.FromArgb("#B8E986"));
                    break;
                case "Vasculitis": // Rojos vivos, cálidos y alegres
                    CargarVistaGenerica("Vasculitis", "globulos.jpg", Color.FromArgb("#FF6F61"), Color.FromArgb("#FF9980"));
                    break;
                default:
                    await DisplayAlert("Error", "No se ha podido cargar el usuario", "OK");
                    break;
            }


        }

        private void CargarVistaGenerica(string nombrePatologia, string imagen, Color color1, Color color2)
        {
            this.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = color1, Offset = 0.0f },
                    new GradientStop { Color = color2, Offset = 1.0f }
                }
            };

            var layout = new StackLayout
            {
                Padding = 20,
                Spacing = 15,
            };

            layout.Children.Add(new Label
            {
                Text = "SintomApp",
                FontSize = 32,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            });

            layout.Children.Add(new Label
            {
                Text = $"Bienvenido, {usuarioInicio.Nombre}",
                FontSize = 20,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            });

            layout.Children.Add(new Image
            {
                Source = imagen,
                Aspect = Aspect.AspectFit,
                HeightRequest = 180,
                WidthRequest = 300,
                HorizontalOptions = LayoutOptions.Center
            });

            var botonesPrincipales = new (string, string, Action)[]
            {
                ("Cuestionario", "📋", async () => await IrACuestionario()),
                ("Calendario", "📅", async () => await Shell.Current.GoToAsync("UserCalendar")),
                ("Ajustes", "⚙️", async () => await Shell.Current.GoToAsync("NewUser"))
            };
            Questill.FechaEsEncuesta(usuarioInicio.PrimerDia?? DateTime.Today, (int)usuarioInicio.Frecuencia, DateTime.Today);

            async Task IrACuestionario()
            {
                if (usuarioInicio.PrimerDia == null)
                {
                    await Shell.Current.GoToAsync("Questill1");
                }
                else
                {
                    if (!Questill.FechaEsEncuesta((DateTime)usuarioInicio.PrimerDia, (int)usuarioInicio.Frecuencia, DateTime.Today))
                    {
                        await DisplayAlert("Sin encuesta", "No tienes encuestas asignadas para hoy.", "OK");
                        return;
                    }

                    var dbManager = new DatabaseManager();
                    var estado = await dbManager.EstadoEncuesta(usuarioInicio.Id, DateTime.Today);

                    if (estado == null)
                    {
                        await DisplayAlert("Error", "No se pudo comprobar el estado de la encuesta. Inténtalo más tarde.", "OK");
                        return;
                    }
                    if (estado == true)
                    {
                        await DisplayAlert("Encuesta ya realizada", "Ya has completado la encuesta de hoy.", "OK");
                        return;
                    }

                }

                await Shell.Current.GoToAsync("Questill1");
            }


            var gridPrincipal = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                RowSpacing = 20,
                ColumnSpacing = 20,
                HorizontalOptions = LayoutOptions.Center
            };

            for (int i = 0; i < botonesPrincipales.Length; i++)
            {
                var (texto, icono, accion) = botonesPrincipales[i];

                var iconLabel = new Label
                {
                    Text = icono,
                    FontSize = 40, // Tamaño del ícono
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Colors.White
                };


                var stack = new VerticalStackLayout
                {
                    Spacing = 2,
                    Children = { iconLabel },
                    Padding = 0,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                var buttonFrame = new Frame
                {
                    BackgroundColor = Colors.Transparent,
                    CornerRadius = 50,
                    HeightRequest = 100,
                    WidthRequest = 100,
                    Content = stack,
                    Padding = 0,
                    HasShadow = false,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => accion();
                buttonFrame.GestureRecognizers.Add(tapGesture);

                gridPrincipal.Add(buttonFrame, i % 2, i / 2);
            }


            layout.Children.Add(gridPrincipal);

            var gridInferior = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                Margin = new Thickness(0, 30, 0, 20)
            };

            var iconosInferiores = new (string icono, Action accion)[]
            {
                ("ℹ️", () => Navigation.PushAsync(new InfoPage())),
                ("👥", async () => await Launcher.OpenAsync("https://t.me/+e8gUHY8FIOdjMWU0")),
                ("💬", async () => await  Launcher.OpenAsync("https://t.me/+e8gUHY8FIOdjMWU0")), // Grupo del pacientes
                ("🤝", async () => await Launcher.OpenAsync("https://t.me/+e8gUHY8FIOdjMWU0")), // Grupo del hospital
                ("🚪", async () =>
                {
                    bool confirmar = await DisplayAlert("Confirmar", "¿Estás seguro de que deseas cerrar sesión?", "Sí", "No");
                    if (confirmar)
                    {
                        SessionManager.UsuarioActual = null; // Limpiamos la sesión actual
                        await Shell.Current.GoToAsync("//MainPage");
                    }
                })
            };
             
            for (int i = 0; i < iconosInferiores.Length; i++)
            {
                var (icono, accion) = iconosInferiores[i];
                var iconLabel = new Label { Text = icono, FontSize = 28, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => accion();
                iconLabel.GestureRecognizers.Add(tapGesture);
                gridInferior.Add(iconLabel, i, 0);
            }

            layout.Children.Add(gridInferior);

            var scroll = new ScrollView { Content = layout };

            MainLayout.Children.Clear();
            MainLayout.Children.Add(scroll);
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarUsuario();
        }




    }
}
public static class LabelExtensions
{
    public static ImageSource ToImageSource(this Label label)
    {
        var renderer = new Label { Text = label.Text, FontSize = label.FontSize };
        return ImageSource.FromStream(() => new MemoryStream()); // Placeholder
    }
}