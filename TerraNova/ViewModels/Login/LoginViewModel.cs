using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Services;
using TerraNova.Constantes;

namespace TerraNova.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ServicioAutenticacion Autenticacion;

        public LoginViewModel(ServicioAutenticacion autenticacion)
        {
            Autenticacion = autenticacion;
        }

        [ObservableProperty]
        public partial string NombreUsuario { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Contrasena { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string MensajeError { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool EstaCargando { get; set; }

        // Servidor (API) al que se conecta la app; se guarda en el dispositivo al iniciar sesion
        [ObservableProperty]
        public partial string UrlServidor { get; set; } = Configuracion.UrlApi;

        [RelayCommand]
        private void RestablecerServidor()
        {
            UrlServidor = Configuracion.UrlPorDefecto;
        }

        [RelayCommand]
        private async Task IniciarSesion()
        {
            MensajeError = string.Empty;
            if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(Contrasena))
            {
                MensajeError = "Ingrese usuario y contraseña.";
                return;
            }

            var Servidor = Configuracion.NormalizarUrl(UrlServidor);
            if (Servidor == null)
            {
                MensajeError = "La dirección del servidor no es válida. Ejemplo: https://midominio.com";
                return;
            }
            UrlServidor = Servidor;
            Configuracion.GuardarUrlApi(Servidor);

            try
            {
                EstaCargando = true;
                if (await Autenticacion.Login(NombreUsuario, Contrasena))
                {
                    Contrasena = string.Empty;
                    (Shell.Current as AppShell)?.ActualizarMenu();
                    await Shell.Current.GoToAsync(Rutas.Inicio);
                }
                else
                {
                    MensajeError = "Usuario o contraseña incorrectos.";
                }
            }
            catch (Exception Ex)
            {
                MensajeError = $"No se pudo conectar con la API ({Configuracion.UrlApi}). {Ex.Message}";
            }
            finally
            {
                EstaCargando = false;
            }
        }
    }
}
