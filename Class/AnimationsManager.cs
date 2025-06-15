using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Xml.Linq;
using SintomApp.Views;

namespace SintomApp.Class
{
    public static class BackgroundColorService
    {
        public static async Task CambiarColorEntry(Entry entry1, string color)
        {
            entry1.BackgroundColor = Color.FromArgb(color);

            // Esperar un tiempo antes de volver a color normal
            //Cambiarlo para que sea un degradado de rosa a blanco si esta mal y de verde a blanco si esta bien
            await Task.Delay(1000);
            entry1.BackgroundColor = Colors.Transparent;

        }
        public static async Task CambiarColorFondo(ContentPage page, string color)
        {
            page.BackgroundColor = Color.FromArgb(color);


            // Esperar un tiempo antes de volver a color normal
            await Task.Delay(1000);
            page.BackgroundColor = Colors.Transparent;
        }
        public static async Task CambiarColorLog(Entry entry1, Entry entry2, string color)
        {
            entry1.BackgroundColor = Color.FromArgb(color);
            entry2.BackgroundColor = Color.FromArgb(color);

            // Esperar un tiempo antes de volver a color normal
            await Task.Delay(1000);
            entry1.BackgroundColor = Colors.Transparent;
            entry2.BackgroundColor = Colors.Transparent;
        }

        public static void ColorFondo(ContentPage page)
        {
            // Restaurar el degradado original
            page.Background = new LinearGradientBrush
            {
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromArgb("#D292DC"), 0.0f),
            new GradientStop(Colors.White, 1.0f)
        },
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
        }
    }

}
