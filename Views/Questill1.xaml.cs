using SintomApp.ViewModels;
using SintomApp.Class;
using System.Text.Json;

namespace SintomApp.Views
{
    public partial class Questill1 : ContentPage
    {
        private readonly PreguntaViewModel _viewModel;

        public Questill1()
        {
            InitializeComponent();
            _viewModel = new PreguntaViewModel(SessionManager.UsuarioActual);
            BindingContext = _viewModel;

            this.BackgroundColor = Colors.Transparent;
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            CambiarTitulo();
            await _viewModel.CargarPreguntasAsync();
            MostrarPregunta();
        }

        private void CambiarTitulo()
        {
            switch (_viewModel.Usuario.Patologia)
            {
                case "LES":
                    Title.Text = "SintomApp LES";
                    break;
                case "Miositis":
                    Title.Text = "SintomApp Miositis";
                    break;
                case "Esclerodermia":
                    Title.Text = "SintomApp Esclerodermia";
                    break;
                case "Vasculitis":
                    Title.Text = "SintomApp Vasculitis";
                    break;
                default:
                    Title.Text = "SintomApp";
                    break;
            }
        }

        private void MostrarPregunta()
        {
            PreguntaActualLayout.Children.Clear();
            var pregunta = _viewModel.PreguntaActual;
            if (pregunta == null) return;

            if (_viewModel.Usuario.Patologia == "Vasculitis")
                MostrarPreguntaVasculitis(pregunta);
            else
                MostrarPreguntaGenerica(pregunta);
        }

        private void MostrarPreguntaGenerica(Pregunta pregunta)
        {
            BtnSiguiente.IsEnabled = false;

            var grid = new Grid
            {
                RowSpacing = 10,
                ColumnSpacing = 10,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            var label = new Label
            {
                Text = pregunta.Texto,
                FontSize = 24,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Justify
            };
            grid.Add(label, 0, 0);

            var contenedorExtra = new StackLayout();
            grid.Add(contenedorExtra, 0, 2);

            var layoutBool = CrearEntradaBool(pregunta, seleccion =>
            {
                contenedorExtra.Children.Clear();

                if (seleccion == "Si")
                {
                    View entradaExtra = pregunta.Tipo?.ToLower() switch
                    {
                        "string2" => CrearEntradaString2(pregunta),
                        "string3" => CrearEntradaString3(pregunta),
                        "int" => CrearEntradaInt(pregunta),
                        _ => null
                    };

                    if (entradaExtra != null)
                        contenedorExtra.Children.Add(entradaExtra);
                    else if (pregunta.Tipo?.ToLower() == "bool")
                    {
                        _viewModel.RegistrarRespuesta(pregunta.Id, "Sí",null,null, pregunta.Puntuacion);
                        BtnSiguiente.IsEnabled = true;
                    }
                }
                else
                {
                    _viewModel.RegistrarRespuesta(pregunta.Id, "No", 0);
                    BtnSiguiente.IsEnabled = true;
                }
            });

            grid.Add(layoutBool, 0, 1);
            PreguntaActualLayout.Children.Add(grid);

            BtnSiguiente.Text = _viewModel.EsUltimaPregunta() ? "Enviar" : "Siguiente";
        }

        private View CrearEntradaBool(Pregunta pregunta, Action<string> onSeleccion)
        {
            var layout = new Grid
            {
                RowDefinitions = { new RowDefinition(), new RowDefinition() },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var checkSi = new CheckBox();
            var checkNo = new CheckBox();

            checkSi.CheckedChanged += (s, e) =>
            {
                if (e.Value)
                {
                    checkNo.IsChecked = false;
                    onSeleccion("Si");
                }
            };

            checkNo.CheckedChanged += (s, e) =>
            {
                if (e.Value)
                {
                    checkSi.IsChecked = false;
                    onSeleccion("No");
                }
            };

            layout.Add(new Label { Text = "Sí", FontSize = 20 }, 0, 0);
            layout.Add(checkSi, 1, 0);
            layout.Add(new Label { Text = "No", FontSize = 20 }, 0, 1);
            layout.Add(checkNo, 1, 1);

            return layout;
        }

        private View CrearEntradaString2(Pregunta pregunta)
        {
            var layout = new VerticalStackLayout { Spacing = 10 };
            layout.Children.Add(new Label { Text = pregunta.Texto2 });

            var entry = new Entry { Placeholder = "Respuesta", TextColor = Colors.Black };
            entry.TextChanged += (_, _) =>
            {
                if (!string.IsNullOrWhiteSpace(entry.Text))
                {
                    _viewModel.RegistrarRespuesta(pregunta.Id, "Sí", entry.Text, null, pregunta.Puntuacion);
                    BtnSiguiente.IsEnabled = true;
                }
                else BtnSiguiente.IsEnabled = false;
            };

            layout.Children.Add(entry);
            return layout;
        }

        private View CrearEntradaString3(Pregunta pregunta)
        {
            var layout = new VerticalStackLayout { Spacing = 10 };
            var entry1 = new Entry { Placeholder = "Respuesta 1", TextColor = Colors.Black };
            var entry2 = new Entry { Placeholder = "Respuesta 2", TextColor = Colors.Black };

            void Validar()
            {
                if (!string.IsNullOrWhiteSpace(entry1.Text) && !string.IsNullOrWhiteSpace(entry2.Text))
                {
                    _viewModel.RegistrarRespuesta(pregunta.Id, "Sí", entry1.Text, entry2.Text, pregunta.Puntuacion);
                    BtnSiguiente.IsEnabled = true;
                }
            }

            entry1.TextChanged += (_, _) => Validar();
            entry2.TextChanged += (_, _) => Validar();

            layout.Children.Add(new Label { Text = pregunta.Texto2 });
            layout.Children.Add(entry1);
            layout.Children.Add(new Label { Text = pregunta.Texto3 });
            layout.Children.Add(entry2);

            return layout;
        }

        private View CrearEntradaInt(Pregunta pregunta)
        {
            var layout = new VerticalStackLayout { Spacing = 10 };
            layout.Children.Add(new Label { Text = pregunta.Texto2 });

            var entry = new Entry { Placeholder = "Número", Keyboard = Keyboard.Numeric, TextColor = Colors.Black };
            entry.TextChanged += (_, _) =>
            {
                if (int.TryParse(entry.Text, out _))
                {
                    _viewModel.RegistrarRespuesta(pregunta.Id, "Sí",entry.Text, null, pregunta.Puntuacion);
                    BtnSiguiente.IsEnabled = true;
                }
                else BtnSiguiente.IsEnabled = false;
            };

            layout.Children.Add(entry);
            return layout;
        }

        private void MostrarPreguntaVasculitis(Pregunta pregunta)
        {
            BtnSiguiente.IsEnabled = false;
            var grid = new Grid
            {
                RowSpacing = 10,
                ColumnSpacing = 10,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            var label = new Label
            {
                Text = pregunta.Texto,
                FontSize = 24,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
                HorizontalTextAlignment = TextAlignment.Justify
            };
            grid.Add(label, 0, 0);

            var contenedorExtra = new StackLayout();
            grid.Add(contenedorExtra, 0, 2);

            var layoutBool = CrearEntradaBool(pregunta, seleccion =>
            {
                contenedorExtra.Children.Clear();
                if (seleccion == "Si")
                {
                    var picker = new Picker
                    {
                        Title = "Seleccione una opción",
                        ItemsSource = new List<string> { "Persistente", "Nuevas o empeoran" },
                        BackgroundColor = Colors.Transparent
                    };

                    picker.SelectedIndexChanged += (_, _) =>
                    {
                        if (picker.SelectedIndex >= 0)
                        {
                            var seleccion = picker.SelectedItem.ToString();
                            decimal puntuacion = seleccion switch
                            {
                                "Persistente" => pregunta.PuntuacionPersistente ?? 0,
                                "Nuevas o empeoran" => pregunta.PuntuacionNueva ?? 0,
                                _ => 0
                            };

                            //var json = JsonSerializer.Serialize(new { respuesta = "Sí", detalle = seleccion });

                            _viewModel.RegistrarRespuesta(pregunta.Id, "Sí", seleccion,null, puntuacion);
                            BtnSiguiente.IsEnabled = true;
                        }
                    };

                    contenedorExtra.Children.Add(picker);

                }
                else
                {
                    _viewModel.RegistrarRespuesta(pregunta.Id, "No", 0);
                    BtnSiguiente.IsEnabled = true;
                }

            });

            grid.Add(layoutBool, 0, 1);
            PreguntaActualLayout.Children.Add(grid);

            BtnSiguiente.Text = _viewModel.EsUltimaPregunta() ? "Enviar" : "Siguiente";
        }

        private async void SiguienteBtnClicked(object sender, EventArgs e)
        {
            if (_viewModel.AvanzarPregunta()) // Cambiar a un método async que devuelva un bool
                MostrarPregunta();
            else
            { 
                bool exito = await _viewModel.GuardarRespuestasAsync();
                if (exito)
                {
                    await DisplayAlert("Éxito", "Respuestas guardadas correctamente.", "OK");
                    await Shell.Current.GoToAsync("../..");
                }
                else
                {
                    await DisplayAlert("Error", "Error al guardar respuestas.", "OK");
                }
            }
        }
        
    }
}
