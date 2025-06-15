using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
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

                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    await DisplayAlert("Guardado", $"El archivo se guardó en:\n{filePath}", "OK");
                    await Launcher.Default.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(filePath)
                    });
                }
                else
                {
                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = "Compartir archivo HTML",
                        File = new ShareFile(filePath)
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
            }
            await Shell.Current.GoToAsync(".."); // Volver a la página anterior
        }
        private async void VovlerClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Confirmar", "Pulse <Si> para volver.", "Sí", "No");
            if (confirmar)
            {
                await Shell.Current.GoToAsync("//Admin");
            }
        }
    }
}
