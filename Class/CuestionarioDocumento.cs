
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class CuestionarioDocumento : IDocument
{
    private readonly Usuario _usuario;
    private readonly Dictionary<Pregunta, string> _respuestas;

    public  CuestionarioDocumento(Usuario user, Dictionary<Pregunta, string> respuestas)
    {
        _usuario = user;
        _respuestas = respuestas;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);
            page.Header().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Paciente: {_usuario.Nombre} {_usuario.Apellidos}").FontSize(20).Bold();
                    col.Item().Text($"Patología: {_usuario.Patologia}").FontSize(16);
                    col.Item().Text($"Fech: {DateTime.Now:dd/MM/yyyy}").FontSize(12);
                });
                //row.ConstantItem(80).Height(80).Element("logo.png").FitArea());
            });

            page.Content().PaddingVertical(10).Column(col =>
            {
                foreach (var item in _respuestas)
                {
                    col.Item().Text(text =>
                    {
                        text.Span($"• {item.Key.Texto}:").SemiBold();
                        text.Span($" {item.Value}");
                    });
                    if (!string.IsNullOrEmpty(item.Key.Texto2))
                        col.Item().Text($"↳ {item.Key.Texto2}").FontSize(10).Italic();
                    if (!string.IsNullOrEmpty(item.Key.Texto3))
                        col.Item().Text($"↳ {item.Key.Texto3}").FontSize(10).Italic();
                }
            });
            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("ArtDamage - Informe generado automáticamente")
                 .FontSize(10)
                 .Italic()
                 .FontColor("#555");
            });
        });
    }
}
