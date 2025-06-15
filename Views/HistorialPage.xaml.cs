using SintomApp.Class; // Asegúrate de usar los namespaces correctos
using SintomApp.Views;
using MySqlConnector;

namespace SintomApp.Views;

public partial class HistorialPage : ContentPage
{
    private readonly Usuario _usuario;
    private readonly DatabaseManager _dbManager;

    public HistorialPage()
    {
        InitializeComponent();
        _usuario = SessionManager.UsuarioSeleccionado;
        _dbManager = new DatabaseManager();
        CargarHistorial();
    }

    private async void CargarHistorial()
    {
        try
        {
            var historial = await _dbManager.ObtenerHistorialPorPregunta(_usuario);

            foreach (var entrada in historial)
            {
                var pregunta = entrada.Key;
                var respuestas = entrada.Value;

                var boton = new Button
                {
                    Text = pregunta.Texto,
                    BackgroundColor = Colors.LightGray,
                    TextColor = Colors.Black,
                    CornerRadius = 12
                };

                boton.Clicked += async (s, e) =>
                {
                    string historialTexto = string.Join("\n\n", respuestas.Select(r =>
                        $"📅 {r.fecha:dd/MM/yyyy} - 📋 {r.respuesta}"));

                    await DisplayAlert("Historial", historialTexto, "Cerrar");
                };

                PreguntasLayout.Children.Add(boton);
            }

            if (!historial.Any())
            {
                PreguntasLayout.Children.Add(new Label
                {
                    Text = "No hay historial disponible.",
                    FontSize = 18,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar el historial: {ex.Message}", "Cerrar");
        }
    }
}
