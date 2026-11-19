using TerraNova.Constantes;
using TerraNova.Services;
using TerraNova.Views;

namespace TerraNova
{
    public partial class AppShell : Shell
    {
        private readonly ServicioAutenticacion Autenticacion;

        public AppShell(ServicioAutenticacion autenticacion)
        {
            InitializeComponent();
            Autenticacion = autenticacion;

            // Paginas a las que se navega con GoToAsync (no estan en el menu)
            Routing.RegisterRoute(Rutas.DetallePropiedad, typeof(DetallePropiedadPage));
            Routing.RegisterRoute(Rutas.PropiedadForm, typeof(PropiedadFormPage));
            Routing.RegisterRoute(Rutas.ClienteForm, typeof(ClienteFormPage));
            Routing.RegisterRoute(Rutas.TrabajadorForm, typeof(TrabajadorFormPage));
            Routing.RegisterRoute(Rutas.UsuarioForm, typeof(UsuarioFormPage));
            Routing.RegisterRoute(Rutas.VentaForm, typeof(VentaFormPage));
            Routing.RegisterRoute(Rutas.VisitaForm, typeof(VisitaFormPage));

            ActualizarMenu();
        }

        // Muestra el usuario conectado y oculta "Usuarios" si no es Admin (igual que MainLayout.razor)
        public void ActualizarMenu()
        {
            ItemUsuarios.IsVisible = Autenticacion.EsAdmin;
            var U = Autenticacion.UsuarioActual;
            LblUsuario.Text = U?.NombreUsuario ?? string.Empty;
            LblRol.Text = U?.Rol ?? string.Empty;
        }

        private async void CerrarSesion(object? sender, EventArgs e)
        {
            if (!await DisplayAlertAsync("Cerrar sesión", "¿Desea salir de TerraNova?", "Sí", "No")) return;
            Autenticacion.CerrarSesion();
            FlyoutIsPresented = false;
            ActualizarMenu();
            await GoToAsync(Rutas.Login);
        }
    }
}
