using SintomApp.Class;
using System.Collections.ObjectModel;

namespace SintomApp.Views;

public partial class RespuestasPregunta : ContentPage
{
    private readonly int _idPregunta;
    private readonly Usuario _usuario;

    public ObservableCollection<RespuestaConFecha> Respuestas { get; set; } = new();

    public RespuestasPregunta()
    {
        InitializeComponent();
        _usuario = SessionManager.UsuarioSeleccionado;
        _idPregunta = SessionManager.PreguntaSeleccionada.Id;

        PreguntaLabel.Text = SessionManager.PreguntaSeleccionada.Texto;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarRespuestas();
    }

    private async Task CargarRespuestas()
    {
        var db = new DatabaseManager();
        var lista = await db.ObtenerRespuestasPorPregunta(_usuario.Id, _idPregunta);
        var ordenadas = lista.OrderByDescending(r => r.Fecha).ToList();

        Respuestas.Clear();
        foreach (var r in ordenadas)
        {
            Respuestas.Add(r);
        }

        RespuestasView.ItemsSource = Respuestas;
    }
}
