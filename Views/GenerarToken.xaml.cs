using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SintomApp.Class;
using System.Net.Mail;
using System.Net;

namespace SintomApp.Views
{
    public partial class GenerarToken : ContentPage
    {
        public GenerarToken()
        {
            InitializeComponent();
        }

        private async void GenerarTokenBtn_Clicked(object sender, EventArgs e)
        {
            string correo = CorreoEntry.Text?.Trim();

            if (string.IsNullOrEmpty(correo))
            {
                await DisplayAlert("Error", "Por favor, introduce un correo válido.", "OK");
                return;
            }

            DatabaseManager dbManager = new DatabaseManager();
            var resultado = await dbManager.GenerarTokenRegistroAsync(correo);

            if (resultado.Exito)
            {
                bool enviado = await EnviarCorreoToken(correo, resultado.Token!); // resultado.Token no es null si Exito es true

                if (enviado)
                {
                    ResultadoLabel.TextColor = Colors.Green;
                    ResultadoLabel.Text = "✅ Token generado y enviado correctamente.";
                }
                else
                {
                    ResultadoLabel.TextColor = Colors.Orange;
                    ResultadoLabel.Text = "⚠️ Token guardado, pero error al enviar el correo.";
                }
            }
            else
            {
                ResultadoLabel.TextColor = Colors.Red;
                ResultadoLabel.Text = $"❌ {resultado.Mensaje}";
            }
        }

        private async Task<bool> EnviarCorreoToken(string correoDestino, string token)
        {
            try
            {
                var mensaje = new MailMessage
                {
                    From = new MailAddress("tu_correo@dominio.com"),
                    Subject = "Token de Registro - ArtDamage",
                    Body = $"Hola,\n\nTu token de registro es: {token}\nEste token es válido por 15 días.\n\nSaludos,\nEquipo de ArtDamage"
                };
                mensaje.To.Add(correoDestino);

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("loginquesta@gmail.com", "btqh cicq uaej kvdz");
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mensaje);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                return false;
            }
        }
    }
}
