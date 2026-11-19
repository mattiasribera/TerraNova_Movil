using System.Net;
using System.Net.Http.Json;
using TerraNova.Shared;

namespace TerraNova.Services
{
    // Todos los endpoints de la API responden ResponseAPI<T> { EsCorrecto, Valor, Mensaje }.
    // Esta clase centraliza esa logica y el manejo de errores para no repetirlos en cada servicio:
    //   401 -> la sesion expiro (se vuelve al login)
    //   403 -> sin permiso (se muestra el mensaje de la API)
    //   sin conexion / API apagada -> mensaje claro en vez del error tecnico
    public abstract class ServicioBase
    {
        // AppShell escucha este evento para cerrar la sesion y volver al login
        public static event Action? SesionExpirada;

        protected readonly HttpClient Http;

        protected ServicioBase(HttpClient http)
        {
            Http = http;
        }

        protected async Task<T> Obtener<T>(string Url)
        {
            var Respuesta = await Enviar(() => Http.GetAsync(Url));
            var Resultado = await Leer<T>(Respuesta);
            if (Resultado != null && Resultado.EsCorrecto)
                return Resultado.Valor!;
            throw new Exception(Resultado?.Mensaje ?? "Error al consultar la API");
        }

        // Para endpoints que devuelven el valor directo, sin ResponseAPI (ej: ExisteCi -> true/false)
        protected async Task<T> ObtenerDirecto<T>(string Url)
        {
            var Respuesta = await Enviar(() => Http.GetAsync(Url));
            return (await Respuesta.Content.ReadFromJsonAsync<T>())!;
        }

        protected Task<int> Crear(string Url, object Dto) => Guardar(() => Http.PostAsJsonAsync(Url, Dto));

        protected Task<int> Actualizar(string Url, object Dto) => Guardar(() => Http.PutAsJsonAsync(Url, Dto));

        protected Task<int> Borrar(string Url) => Guardar(() => Http.DeleteAsync(Url));

        private async Task<int> Guardar(Func<Task<HttpResponseMessage>> Peticion)
        {
            var Respuesta = await Enviar(Peticion);
            var Resultado = await Leer<int>(Respuesta);
            if (Resultado != null && Resultado.EsCorrecto)
                return Resultado.Valor;
            throw new Exception(Resultado?.Mensaje ?? $"Error del servidor ({(int)Respuesta.StatusCode})");
        }

        // Ejecuta la peticion y traduce los errores de red/sesion a mensajes entendibles
        private static async Task<HttpResponseMessage> Enviar(Func<Task<HttpResponseMessage>> Peticion)
        {
            HttpResponseMessage Respuesta;
            try
            {
                Respuesta = await Peticion();
            }
            catch (Exception Ex) when (Ex is HttpRequestException or TaskCanceledException)
            {
                throw new Exception(Connectivity.Current.NetworkAccess != NetworkAccess.Internet
                    ? "No hay conexión a internet. Revise su Wi-Fi o datos móviles."
                    : "No se pudo conectar con el servidor de TerraNova. Verifique que la API esté en ejecución.");
            }

            if (Respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                MainThread.BeginInvokeOnMainThread(() => SesionExpirada?.Invoke());
                throw new Exception("Su sesión expiró. Vuelva a iniciar sesión.");
            }
            if (Respuesta.StatusCode == HttpStatusCode.Forbidden)
            {
                var Detalle = await Leer<object>(Respuesta);
                throw new Exception(Detalle?.Mensaje ?? "No tiene permiso para realizar esta acción.");
            }
            return Respuesta;
        }

        private static async Task<ResponseAPI<T>?> Leer<T>(HttpResponseMessage Respuesta)
        {
            try { return await Respuesta.Content.ReadFromJsonAsync<ResponseAPI<T>>(); }
            catch { return null; }
        }
    }
}
