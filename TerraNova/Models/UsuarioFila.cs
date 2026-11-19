using TerraNova.Shared;

namespace TerraNova.Models
{
    // Fila de la lista: usuario + nombre del trabajador vinculado
    public class UsuarioFila
    {
        public UsuarioDTO Usuario { get; set; } = new();
        public string Trabajador { get; set; } = "—";
        public bool EsAdmin => Usuario.Rol == "Admin";
    }
}
