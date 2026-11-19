using System.Net.Http.Json;
using TerraNova.Shared;

namespace TerraNova.Services
{
    public class ServicioClientes : ServicioBase
    {
        public ServicioClientes(HttpClient http) : base(http) { }

        public Task<List<ClienteDTO>> Lista() => Obtener<List<ClienteDTO>>("api/Cliente/Lista");
        public Task<ClienteDTO> Buscar(int Cod) => Obtener<ClienteDTO>($"api/Cliente/Buscar/{Cod}");
        public Task<int> Guardar(ClienteDTO C) => Crear("api/Cliente/Guardar", C);
        public Task<int> Modificar(ClienteDTO C) => Actualizar($"api/Cliente/Modificar/{C.Id}", C);
        public Task<int> Eliminar(int Cod) => Borrar($"api/Cliente/Eliminar/{Cod}");

        // Validaciones de duplicados (devuelven true/false directo, sin ResponseAPI)
        public Task<bool> ExisteCi(string Ci, int IdExcluir) =>
            Http.GetFromJsonAsync<bool>($"api/Cliente/ExisteCi/{Uri.EscapeDataString(Ci)}/{IdExcluir}");
        public Task<bool> ExisteTelefono(string Telefono, int IdExcluir) =>
            Http.GetFromJsonAsync<bool>($"api/Cliente/ExisteTelefono/{Uri.EscapeDataString(Telefono)}/{IdExcluir}");
        public Task<bool> ExisteEmail(string Email, int IdExcluir) =>
            Http.GetFromJsonAsync<bool>($"api/Cliente/ExisteEmail/{Uri.EscapeDataString(Email)}/{IdExcluir}");
    }
}
