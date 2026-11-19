using TerraNova.Constantes;
using TerraNova.Services;

namespace TerraNova
{
    public partial class App : Application
    {
        private readonly ServicioAutenticacion Autenticacion;

        public App(ServicioAutenticacion autenticacion)
        {
            InitializeComponent();
            Autenticacion = autenticacion;

            // La app esta diseñada en tema claro: evita que el modo oscuro de Windows/Android cambie los colores
            UserAppTheme = AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var Shell = new AppShell(Autenticacion);

            // Si ya habia sesion guardada, entra directo al Inicio (dashboard)
            if (Autenticacion.EstaLogueado)
                Shell.Dispatcher.Dispatch(async () => await Shell.GoToAsync(Rutas.Inicio));

            var Ventana = new Window(Shell) { Title = "TerraNova Bienes Raíces" };

            // En Windows: tamaño comodo para presentar, centrado en la pantalla y con tamaño minimo
            if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
            {
                const double Ancho = 1280, Alto = 800;
                var Pantalla = DeviceDisplay.Current.MainDisplayInfo;
                Ventana.Width = Ancho;
                Ventana.Height = Alto;
                Ventana.MinimumWidth = 900;
                Ventana.MinimumHeight = 600;
                Ventana.X = Math.Max(0, (Pantalla.Width / Pantalla.Density - Ancho) / 2);
                Ventana.Y = Math.Max(0, (Pantalla.Height / Pantalla.Density - Alto) / 2);

                // Barra de titulo de Windows con los colores de TerraNova (en vez del gris por defecto)
                Ventana.TitleBar = new TitleBar
                {
                    Title = "TerraNova Bienes Raíces",
                    BackgroundColor = Color.FromArgb("#153D23"),
                    ForegroundColor = Colors.White,
                    HeightRequest = 36
                };
            }
            return Ventana;
        }
    }
}
