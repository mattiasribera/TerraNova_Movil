using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TerraNova.Shared;

namespace TerraNova.Services
{
    // Maneja la sesion: login, token JWT y cierre de sesion.
    // En la web se usaba Blazored.LocalStorage; en MAUI el equivalente es Preferences.
    public class ServicioAutenticacion
    {
        private const string ClaveStorage = "usuarioActual";
        private readonly HttpClient Http;

        public UsuarioDTO? UsuarioActual { get; private set; }
        public bool EstaLogueado => UsuarioActual != null && !string.IsNullOrEmpty(UsuarioActual.Token);
        public bool EsAdmin => UsuarioActual?.Rol == "Admin";

        public ServicioAutenticacion(HttpClient http)
        {
            Http = http;
            var Guardado = Preferences.Default.Get(ClaveStorage, string.Empty);
            if (!string.IsNullOrEmpty(Guardado))
            {
                UsuarioActual = JsonSerializer.Deserialize<UsuarioDTO>(Guardado);
                AplicarToken();
            }
        }

        public async Task<bool> Login(string NombreUsuario, string Contrasena)
        {
            HttpResponseMessage Resultado;
            try
            {
                Resultado = await Http.PostAsJsonAsync("api/Usuario/Login", new LoginRequestDTO
                {
                    NombreUsuario = NombreUsuario,
                    Contrasena = Contrasena
                });
            }
            catch (Exception Ex) when (Ex is HttpRequestException or TaskCanceledException)
            {
                throw new Exception(Connectivity.Current.NetworkAccess != NetworkAccess.Internet
                    ? "No hay conexión a internet."
                    : "No se pudo conectar con el servidor de TerraNova.");
            }

            var Respuesta = await Resultado.Content.ReadFromJsonAsync<ResponseAPI<UsuarioDTO>>();
            if (Respuesta == null || !Respuesta.EsCorrecto || string.IsNullOrEmpty(Respuesta.Valor?.Token))
                return false;

            UsuarioActual = Respuesta.Valor;
            Preferences.Default.Set(ClaveStorage, JsonSerializer.Serialize(UsuarioActual));
            AplicarToken();
            return true;
        }

        public void CerrarSesion()
        {
            UsuarioActual = null;
            Preferences.Default.Remove(ClaveStorage);
            AplicarToken();
        }

        // Todas las peticiones a la API llevan "Authorization: Bearer <token>"
        private void AplicarToken() =>
            Http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(UsuarioActual?.Token)
                ? null
                : new AuthenticationHeaderValue("Bearer", UsuarioActual.Token);
    }
}
