using TerraNova.Shared;

namespace TerraNova.Models
{
    public class ActividadReciente
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty; // codigo de Constantes.Iconos
        public DateTime Fecha { get; set; }
    }
}
