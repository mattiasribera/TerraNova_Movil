using System.Net.Http.Json;
using TerraNova.Shared;

namespace TerraNova.Services
{
    // Todos los endpoints de la API responden ResponseAPI<T> { EsCorrecto, Valor, Mensaje }.
    // Esta clase centraliza esa logica para no repetirla en cada servicio.
    public abstract class ServicioBase
    {
        protected readonly HttpClient Http;

        protected ServicioBase(HttpClient http)
        {
            Http = http;
        }

        protected async Task<T> Obtener<T>(string Url)
        {
            var Resultado = await Http.GetFromJsonAsync<ResponseAPI<T>>(Url);
            if (Resultado != null && Resultado.EsCorrecto)
                return Resultado.Valor!;
            throw new Exception(Resultado?.Mensaje ?? "Error al consultar la API");
        }

        protected Task<int> Crear(string Url, object Dto) => Enviar(Http.PostAsJsonAsync(Url, Dto));

        protected Task<int> Actualizar(string Url, object Dto) => Enviar(Http.PutAsJsonAsync(Url, Dto));

        protected Task<int> Borrar(string Url) => Enviar(Http.DeleteAsync(Url));

        private static async Task<int> Enviar(Task<HttpResponseMessage> Peticion)
        {
            var Respuesta = await Peticion;
            ResponseAPI<int>? Resultado = null;
            try { Resultado = await Respuesta.Content.ReadFromJsonAsync<ResponseAPI<int>>(); } catch { }

            if (Resultado != null && Resultado.EsCorrecto)
                return Resultado.Valor;
            throw new Exception(Resultado?.Mensaje ?? $"Error del servidor ({(int)Respuesta.StatusCode})");
        }
    }
}
