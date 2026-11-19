using System.Net.Http.Json;
using TerraNova.Shared;

namespace TerraNova.Services
{
    public class ServicioVentas : ServicioBase
    {
        public ServicioVentas(HttpClient http) : base(http) { }

        public Task<List<VentaDTO>> Lista() => Obtener<List<VentaDTO>>("api/Venta/Lista");
        public Task<VentaDTO> Buscar(int Cod) => Obtener<VentaDTO>($"api/Venta/Buscar/{Cod}");
        public Task<int> Guardar(VentaDTO V) => Crear("api/Venta/Guardar", V);
        public Task<int> Modificar(VentaDTO V) => Actualizar($"api/Venta/Modificar/{V.Id}", V);
        public Task<int> Eliminar(int Cod) => Borrar($"api/Venta/Eliminar/{Cod}");
    }
}
