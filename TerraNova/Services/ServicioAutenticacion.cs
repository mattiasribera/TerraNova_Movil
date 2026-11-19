using System.Net.Http.Json;
using System.Text.Json;
using TerraNova.Shared;

namespace TerraNova.Services
{
    // En la web se usaba Blazored.LocalStorage; en MAUI el equivalente es Preferences.
    public class ServicioAutenticacion
    {
        private const string ClaveStorage = "usuarioActual";
        private readonly HttpClient Http;

        public UsuarioDTO? UsuarioActual { get; private set; }
        public bool EstaLogueado => UsuarioActual != null;
        public bool EsAdmin => UsuarioActual?.Rol == "Admin";

        public ServicioAutenticacion(HttpClient http)
        {
            Http = http;
            var Guardado = Preferences.Default.Get(ClaveStorage, string.Empty);
            if (!string.IsNullOrEmpty(Guardado))
                UsuarioActual = JsonSerializer.Deserialize<UsuarioDTO>(Guardado);
        }

        public async Task<bool> Login(string NombreUsuario, string Contrasena)
        {
            var Resultado = await Http.PostAsJsonAsync("api/Usuario/Login", new LoginRequestDTO
            {
                NombreUsuario = NombreUsuario,
                Contrasena = Contrasena
            });
            var Respuesta = await Resultado.Content.ReadFromJsonAsync<ResponseAPI<UsuarioDTO>>();
            if (Respuesta == null || !Respuesta.EsCorrecto)
                return false;

            UsuarioActual = Respuesta.Valor;
            Preferences.Default.Set(ClaveStorage, JsonSerializer.Serialize(UsuarioActual));
            return true;
        }

        public void CerrarSesion()
        {
            UsuarioActual = null;
            Preferences.Default.Remove(ClaveStorage);
        }
    }
}
