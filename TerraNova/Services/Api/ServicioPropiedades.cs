using System.Net.Http.Json;
using TerraNova.Shared;

namespace TerraNova.Services
{
    public class ServicioPropiedades : ServicioBase
    {
        public ServicioPropiedades(HttpClient http) : base(http) { }

        public Task<List<PropiedadDTO>> Lista() => Obtener<List<PropiedadDTO>>("api/Propiedad/Lista");
        public Task<List<PropiedadDTO>> Disponibles() => Obtener<List<PropiedadDTO>>("api/Propiedad/Disponibles");
        public Task<PropiedadDTO> Buscar(int Cod) => Obtener<PropiedadDTO>($"api/Propiedad/Buscar/{Cod}");
        public Task<int> Guardar(PropiedadDTO P) => Crear("api/Propiedad/Guardar", P);
        public Task<int> Modificar(PropiedadDTO P) => Actualizar($"api/Propiedad/Modificar/{P.Id}", P);
        public Task<int> Eliminar(int Cod) => Borrar($"api/Propiedad/Eliminar/{Cod}");
    }
}
