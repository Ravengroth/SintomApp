using Microsoft.Maui.Controls;
using System;
using System.IO;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace SintomApp.Views
{
    public partial class HTMLPage : ContentPage
    {
        private readonly DateTime _fecha;
        private readonly string _htmlContent;

        public HTMLPage()
        {
            InitializeComponent();
            _fecha = SessionManager.FechaSeleccionada;
            _htmlContent = SessionManager.HtmlRespuestas;

            var htmlSource = new HtmlWebViewSource
            {
                Html = _htmlContent
            };

            HtmlViewer.Source = htmlSource;
        }

        private async void OnGuardarHtmlClicked(object sender, EventArgs e)
        {
            try
            {
                string filename = $"respuestas_{_fecha:yyyyMMdd}.html";
                string filePath = Path.Combine(FileSystem.AppDataDirectory, filename);

                File.WriteAllText(filePath, _htmlContent);

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Compartir archivo HTML",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
            }
            await Shell.Current.GoToAsync(".."); // Volver a la página anterior
        }
    }
}
