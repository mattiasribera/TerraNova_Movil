using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using TerraNova.Services;
using TerraNova.ViewModels;
using TerraNova.Views;

namespace TerraNova
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Formato de Bolivia en toda la app ($ 185.000, 3,5 %, fechas en español),
            // sin importar el idioma configurado en el celular o en Windows
            // Cualquier error no controlado queda en errores.log (ver Services/RegistroErrores.cs)
            RegistroErrores.Activar();

            var Bolivia = new System.Globalization.CultureInfo("es-BO");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = Bolivia;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = Bolivia;
            System.Globalization.CultureInfo.CurrentCulture = Bolivia;
            System.Globalization.CultureInfo.CurrentUICulture = Bolivia;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                // INVESTIGACION: activa CommunityToolkit.Maui.
                // En Windows el Toast viene desactivado y hay que habilitarlo con esta opcion.
                .UseMauiCommunityToolkit(options => options.SetShouldEnableSnackbarOnWindows(true))
                // Mapsui dibuja el mapa con SkiaSharp
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    // Iconos de Windows 11 (Fluent UI System Icons) - ver Constantes/Iconos.cs
                    fonts.AddFont("FluentIcons.ttf", "Iconos");
                });

            // Un solo HttpClient apuntando a TerraNova.Api (equivalente al HttpClient de Blazor.Client/Program.cs)
            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(Configuracion.UrlApi) });

            // Servicios (consumen la API)
            builder.Services.AddSingleton<ServicioAutenticacion>();
            builder.Services.AddSingleton<ServicioPropiedades>();
            builder.Services.AddSingleton<ServicioClientes>();
            builder.Services.AddSingleton<ServicioTrabajadores>();
            builder.Services.AddSingleton<ServicioUsuarios>();
            builder.Services.AddSingleton<ServicioVentas>();
            builder.Services.AddSingleton<ServicioVisitas>();
            builder.Services.AddSingleton<DashboardService>();
            builder.Services.AddSingleton<ServicioImagenes>();

            // Paginas con su ViewModel (Transient: se crea uno nuevo cada vez que se abre)
            Registrar<LoginPage, LoginViewModel>(builder);
            Registrar<DashboardPage, DashboardViewModel>(builder);
            Registrar<PropiedadesPage, PropiedadesViewModel>(builder);
            Registrar<DetallePropiedadPage, DetallePropiedadViewModel>(builder);
            Registrar<PropiedadFormPage, PropiedadFormViewModel>(builder);
            Registrar<ClientesPage, ClientesViewModel>(builder);
            Registrar<ClienteFormPage, ClienteFormViewModel>(builder);
            Registrar<TrabajadoresPage, TrabajadoresViewModel>(builder);
            Registrar<TrabajadorFormPage, TrabajadorFormViewModel>(builder);
            Registrar<UsuariosPage, UsuariosViewModel>(builder);
            Registrar<UsuarioFormPage, UsuarioFormViewModel>(builder);
            Registrar<VentasPage, VentasViewModel>(builder);
            Registrar<VentaFormPage, VentaFormViewModel>(builder);
            Registrar<VisitasPage, VisitasViewModel>(builder);
            Registrar<VisitaFormPage, VisitaFormViewModel>(builder);
            builder.Services.AddTransient<MapaPropiedadesPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void Registrar<TPagina, TViewModel>(MauiAppBuilder builder)
            where TPagina : class where TViewModel : class
        {
            builder.Services.AddTransient<TPagina>();
            builder.Services.AddTransient<TViewModel>();
        }
    }
}
