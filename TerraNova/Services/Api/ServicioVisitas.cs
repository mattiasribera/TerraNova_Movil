using System.Net.Http.Json;
using TerraNova.Shared;

namespace TerraNova.Services
{
    public class ServicioVisitas : ServicioBase
    {
        public ServicioVisitas(HttpClient http) : base(http) { }

        public Task<List<VisitaDTO>> Lista() => Obtener<List<VisitaDTO>>("api/Visita/Lista");
        public Task<List<VisitaDTO>> PorPropiedad(int IdPropiedad) => Obtener<List<VisitaDTO>>($"api/Visita/PorPropiedad/{IdPropiedad}");
        public Task<VisitaDTO> Buscar(int Cod) => Obtener<VisitaDTO>($"api/Visita/Buscar/{Cod}");
        public Task<int> Guardar(VisitaDTO V) => Crear("api/Visita/Guardar", V);
        public Task<int> Modificar(VisitaDTO V) => Actualizar($"api/Visita/Modificar/{V.Id}", V);
        public Task<int> Eliminar(int Cod) => Borrar($"api/Visita/Eliminar/{Cod}");
    }
}
