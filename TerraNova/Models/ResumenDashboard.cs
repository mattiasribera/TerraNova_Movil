using TerraNova.Shared;

namespace TerraNova.Models
{
    public class ResumenDashboard
    {
        public int TotalPropiedades { get; set; }
        public int TotalClientes { get; set; }
        public int TotalVentas { get; set; }
        public int TotalTrabajadores { get; set; }
        public List<PropiedadDTO> UltimasPropiedades { get; set; } = new();
        public List<ActividadReciente> ActividadReciente { get; set; } = new();
    }
}
