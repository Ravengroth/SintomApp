using SintomApp.Class;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SintomApp.Views
{
    public partial class AvisosPage : ContentPage
    {
        private readonly int _idMedico;

        public AvisosPage()
        {
            InitializeComponent();
            _idMedico = SessionManager.UsuarioActual.Id;
            _ = CargarAvisosAsync();
        }

        private async Task CargarAvisosAsync()
        {
            try
            {
                DatabaseManager db = new DatabaseManager();
                List<Aviso> avisos = await db.ObtenerAvisosMedicoAsync(_idMedico);

                if (avisos.Count > 0)
                {
                    foreach (var aviso in avisos)
                    {
                        aviso.LeidoTexto = aviso.Leido ? "Leído" : "No leído";
                        aviso.LeidoColor = aviso.Leido ? Colors.Green : Colors.Red;
                    }

                    AvisosCollectionView.ItemsSource = avisos;
                }
                else
                {
                    // No hay avisos, comprueba si hay respuestas previas
                    var puntuaciones = await db.ObtenerPuntuacionesUsuariosMedico(_idMedico); // implementa este método
                    if (puntuaciones.Count > 0)
                    {
                        AvisosCollectionView.ItemsSource = new List<string> { "✅ No hay notificaciones pendientes." };
                    }
                    else
                    {
                        AvisosCollectionView.ItemsSource = new List<string> { "⚠️ No hay datos disponibles para verificar puntuaciones." };
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudieron cargar los avisos: {ex.Message}", "OK");
            }
        }
    }
}
