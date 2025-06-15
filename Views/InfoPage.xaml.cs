using System.ComponentModel;
using System.Runtime.CompilerServices;
using SintomApp.Class;
using SintomApp.Views;

namespace SintomApp.Views
{
    public partial class InfoPage : ContentPage
    {
        private string patologia;
        private Usuario _usuario = SessionManager.UsuarioActual;

        public InfoPage()
        {
            InitializeComponent();
            patologia = _usuario.Patologia;
            //PatologiaPrincipalLabel.Text = $"Has sido diagnosticado con: {patologia}";
        }

        private void OnLupusClicked(object sender, EventArgs e)
        {
            VasculitisFrame.IsVisible = false;
            MiositisFrame.IsVisible = false;
            EsclerodermiaFrame.IsVisible = false;
            LupusFrame.IsVisible = !LupusFrame.IsVisible;
        }

        private void OnEsclerodermiaClicked(object sender, EventArgs e)
        {
            VasculitisFrame.IsVisible = false;
            MiositisFrame.IsVisible = false;
            EsclerodermiaFrame.IsVisible = !EsclerodermiaFrame.IsVisible;
            LupusFrame.IsVisible = false;
        }

        private void OnMiositisClicked(object sender, EventArgs e)
        {
            VasculitisFrame.IsVisible = false;
            MiositisFrame.IsVisible = !MiositisFrame.IsVisible;
            EsclerodermiaFrame.IsVisible = false;
            LupusFrame.IsVisible = false;
        }

        private void OnVasculitisClicked(object sender, EventArgs e)
        {
            VasculitisFrame.IsVisible = !VasculitisFrame.IsVisible;
            MiositisFrame.IsVisible = false;
            EsclerodermiaFrame.IsVisible = false;
            LupusFrame.IsVisible = false;
        }

    }
}