using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraNova.Shared
{
    public class UsuarioDTO
    {
        public int Id { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string? ContraseñaHash { get; set; }

        public string Rol { get; set; } = string.Empty;

        public int? IdTrabajador { get; set; }

        public bool Activo { get; set; }

        public virtual TrabajadorDTO? IdTrabajadorNavigation { get; set; }

        // Token JWT que devuelve el Login (solo viaja en esa respuesta)
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? Token { get; set; }
    }
}
