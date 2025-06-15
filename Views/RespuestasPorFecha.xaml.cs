using SintomApp.Class;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SintomApp.Views;

public partial class RespuestasPorFecha : ContentPage
{
    public record PreguntaYRespuesta(Pregunta Pregunta, string Respuesta);

    private readonly Usuario _usuario;
    private readonly DateTime _fecha;

    public ObservableCollection<PreguntaYRespuesta> Respuestas { get; set; } = new();

    public RespuestasPorFecha()
    {
        InitializeComponent();
        _usuario = SessionManager.UsuarioSeleccionado;
        _fecha = SessionManager.FechaSeleccionada;
        FechaLabel.Text = $"Respuestas del día: {SessionManager.FechaSeleccionada:dd/MM/yyyy}";
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        //base.OnAppearing();
        await CargarRespuestas();
    }

    public string GenerarHtmlDeRespuestas()
    {
        var sb = new StringBuilder();

        sb.AppendLine("<head>");
        sb.AppendLine("<style>");
        sb.AppendLine("html, body { height: 100%; margin: 0; padding: 0; overflow-y: auto; }");
        sb.AppendLine("body { font-family: Arial; background: linear-gradient(to bottom, #00bfff, #add8e6); padding: 20px; }");
        sb.AppendLine(".titulo { font-size: 24px; font-weight: bold; margin-bottom: 20px; text-align: center; color: white; }");
        sb.AppendLine(".usuario-info { background: rgba(255,255,255,0.8); border-radius: 8px; padding: 10px; margin-bottom: 20px; font-size: 14px; color: #000; }");
        sb.AppendLine(".card { background: white; border-radius: 10px; padding: 15px; margin-bottom: 10px; box-shadow: 2px 2px 5px rgba(0,0,0,0.2); }");
        sb.AppendLine(".pregunta { font-weight: bold; font-size: 16px; }");
        sb.AppendLine(".respuesta { margin-top: 5px; font-size: 14px; color: #555; }");
        sb.AppendLine(".subrespuesta { margin-left: 15px; font-size: 13px; color: #333; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");

        sb.AppendLine("<body>");
        sb.AppendLine($"<div class='titulo'>Respuestas del día: {_fecha:dd/MM/yyyy}</div>");
        sb.AppendLine("<div class='usuario-info'>");
        sb.AppendLine($"<div><strong>Nombre:</strong> {_usuario.Nombre} {_usuario.Apellidos}</div>");
        sb.AppendLine($"<div><strong>Teléfono:</strong> {_usuario.Telefono}</div>");
        sb.AppendLine("</div>");

    

        int total = Respuestas.Count;
        int procesadas = 0, fallidas = 0;

        foreach (var item in Respuestas)
        {
            var pregunta = item.Pregunta;
            var respuesta = item.Respuesta;

            sb.AppendLine("<div class='card'>");
            sb.AppendLine($"<div class='pregunta'>{pregunta.Texto}</div>");

            try
            {
                if (!string.IsNullOrWhiteSpace(respuesta) && (respuesta.TrimStart().StartsWith("{") || respuesta.TrimStart().StartsWith("[")))
                {
                    using var jsonDoc = JsonDocument.Parse(respuesta);
                    var root = jsonDoc.RootElement;

                    if (!string.IsNullOrEmpty(pregunta.Tipo))
                    {
                        if (root.TryGetProperty("principal", out var principal))
                            sb.AppendLine($"<div class='respuesta'>Respuesta principal: {principal.GetString()}</div>");

                        switch (pregunta.Tipo)
                        {
                            case "int":
                                if (root.TryGetProperty("extra1", out var intValue))
                                    sb.AppendLine($"<div class='subrespuesta'>{pregunta.Texto2 ?? "Cantidad"}: {intValue.GetInt32()}</div>");
                                break;

                            case "string2":
                                if (root.TryGetProperty("extra1", out var str2))
                                    sb.AppendLine($"<div class='subrespuesta'>{pregunta.Texto2 ?? "Detalle"}: {str2.GetString()}</div>");
                                break;

                            case "string3":
                                if (root.TryGetProperty("extra1", out var str3_1))
                                    sb.AppendLine($"<div class='subrespuesta'>{pregunta.Texto2 ?? "Detalle 1"}: {str3_1.GetString()}</div>");
                                if (root.TryGetProperty("extra2", out var str3_2))
                                    sb.AppendLine($"<div class='subrespuesta'>{pregunta.Texto3 ?? "Detalle 2"}: {str3_2.GetString()}</div>");
                                break;
                        }
                    }
                    else // Vasculitis u otras sin tipo
                    {
                        if (root.TryGetProperty("principal", out var respBool))
                            sb.AppendLine($"<div class='respuesta'>Síntoma presente: {respBool.GetString()}</div>");

                        if (root.TryGetProperty("detalle", out var detalle))
                            sb.AppendLine($"<div class='subrespuesta'>Tipo: {detalle.GetString()}</div>");
                    }

                    procesadas++;
                }
                else
                {
                    // Si no es JSON, mostrar respuesta cruda
                    sb.AppendLine($"<div class='respuesta'>{System.Net.WebUtility.HtmlEncode(respuesta)}</div>");
                }
                
            }
            catch (Exception ex)
            {
                fallidas++;
                sb.AppendLine($"<div class='respuesta'>⚠ Error al procesar: {System.Net.WebUtility.HtmlEncode(respuesta)}<br>Detalle: {ex.Message}</div>");
            }


            sb.AppendLine("</div>");
        }

        sb.AppendLine($"<div style='margin-top: 30px; text-align: center; color:white;'>Total: {total}, Procesadas: {procesadas}, Fallidas: {fallidas}</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }


    private async void OnVerHtmlClicked(object sender, EventArgs e)
    {
        string html = GenerarHtmlDeRespuestas();
        SessionManager.HtmlRespuestas = html;
        SessionManager.FechaSeleccionada = _fecha;
        await Shell.Current.GoToAsync(nameof(HTMLPage));
    }
    private async void VovlerClicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Confirmar", "Pulse <Si> para volver.", "Sí", "No");
        if (confirmar)
        {
            await Shell.Current.GoToAsync("//Admin");
        }
    }

    private async Task CargarRespuestas()
    {
        var db = new DatabaseManager();
        var resultado = await db.ObtenerPreguntasYRespuestasPorFechaAsync(_usuario, _fecha);

        Respuestas.Clear();
        foreach (var par in resultado)
        {
            Respuestas.Add(new PreguntaYRespuesta(par.Key, par.Value));
        }

        RespuestasView.ItemsSource = Respuestas;
    }
}
