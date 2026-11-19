using System.Net.Http.Json;
using TerraNova.Shared;

namespace TerraNova.Services
{
    public class ServicioTrabajadores : ServicioBase
    {
        public ServicioTrabajadores(HttpClient http) : base(http) { }

        public Task<List<TrabajadorDTO>> Lista() => Obtener<List<TrabajadorDTO>>("api/Trabajador/Lista");
        public Task<List<TrabajadorDTO>> Activos() => Obtener<List<TrabajadorDTO>>("api/Trabajador/Activos");
        public Task<TrabajadorDTO> Buscar(int Cod) => Obtener<TrabajadorDTO>($"api/Trabajador/Buscar/{Cod}");
        public Task<int> Guardar(TrabajadorDTO T) => Crear("api/Trabajador/Guardar", T);
        public Task<int> Modificar(TrabajadorDTO T) => Actualizar($"api/Trabajador/Modificar/{T.Id}", T);
        public Task<int> Eliminar(int Cod) => Borrar($"api/Trabajador/Eliminar/{Cod}"); // baja logica (Activo = false)
    }
}
