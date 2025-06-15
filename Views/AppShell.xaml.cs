using SintomApp.Views;


namespace SintomApp.Views
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas para navegación
            Routing.RegisterRoute("NewUser", typeof(NewUser));
            Routing.RegisterRoute("UserData", typeof(UserData));
            Routing.RegisterRoute("EditUser", typeof(EditUser));
            Routing.RegisterRoute("Admin", typeof(Admin));
            Routing.RegisterRoute("DelUser", typeof(DelUser));
            Routing.RegisterRoute("LoadUser", typeof(LoadUser));
            Routing.RegisterRoute("UserCalendar", typeof(UserCalendar));
            Routing.RegisterRoute("Questill1", typeof(Questill1));
            Routing.RegisterRoute("Cuestionarios", typeof(Cuestionarios));
            Routing.RegisterRoute("Inicio", typeof(Inicio));
            Routing.RegisterRoute("AvisosPage", typeof(AvisosPage));
            Routing.RegisterRoute("GenerarToken", typeof(GenerarToken));
            Routing.RegisterRoute("PreRegistroPage", typeof(PreRegistroPage));
            Routing.RegisterRoute("UserRegister", typeof(UserRegister));
            Routing.RegisterRoute("HistorialPage", typeof(HistorialPage));
            Routing.RegisterRoute("HTMLPage", typeof(HTMLPage));
            Routing.RegisterRoute("InfoPage", typeof(InfoPage));
            Routing.RegisterRoute("RegPreguntas", typeof(RegPreguntas));
            Routing.RegisterRoute("RespuestasPorFecha", typeof(RespuestasPorFecha));
            Routing.RegisterRoute("RespuestasPregunta", typeof(RespuestasPregunta));
        }
    }
}
