using System.Net.Http.Json;
using TerraNova.Shared;

namespace TerraNova.Services
{
    public class ServicioUsuarios : ServicioBase
    {
        public ServicioUsuarios(HttpClient http) : base(http) { }

        public Task<List<UsuarioDTO>> Lista() => Obtener<List<UsuarioDTO>>("api/Usuario/Lista");
        public Task<UsuarioDTO> Buscar(int Cod) => Obtener<UsuarioDTO>($"api/Usuario/Buscar/{Cod}");
        public Task<int> Guardar(UsuarioDTO U) => Crear("api/Usuario/Guardar", U);
        public Task<int> Modificar(UsuarioDTO U) => Actualizar($"api/Usuario/Modificar/{U.Id}", U);
        public Task<int> Eliminar(int Cod) => Borrar($"api/Usuario/Eliminar/{Cod}");
        public Task<bool> ExisteNombreUsuario(string Nombre, int IdExcluir) =>
            Http.GetFromJsonAsync<bool>($"api/Usuario/ExisteNombreUsuario/{Uri.EscapeDataString(Nombre)}/{IdExcluir}");
    }
}
