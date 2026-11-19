using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TerraNova.Shared;

namespace TerraNova.Api.Services
{
    // Reglas de permisos (se validan en el SERVIDOR, no solo en la app):
    //
    //               Admin   Agente
    //  Propiedades  todo    ve todas; crea a su nombre; edita solo las suyas; no elimina
    //  Clientes     todo    ve, crea y edita; no elimina
    //  Ventas       todo    ve solo las suyas; registra a su nombre; edita solo las suyas; no anula
    //  Visitas      todo    ve, crea, edita y elimina solo las suyas
    //  Trabajadores todo    ve la lista; edita solo su perfil (sin comision, cargo ni estado)
    //  Usuarios     todo    sin acceso
    public static class Permisos
    {
        public const string RolAdmin = "Admin";

        public static bool EsAdmin(this ClaimsPrincipal Usuario) => Usuario.IsInRole(RolAdmin);

        // Trabajador vinculado al usuario conectado (null si no tiene)
        public static int? IdTrabajador(this ClaimsPrincipal Usuario) =>
            int.TryParse(Usuario.FindFirstValue(ServicioToken.ClaimIdTrabajador), out var Id) ? Id : null;

        // true si es Admin o si el registro pertenece a su trabajador
        public static bool PuedeGestionar(this ClaimsPrincipal Usuario, int IdTrabajadorDelRegistro) =>
            Usuario.EsAdmin() || Usuario.IdTrabajador() == IdTrabajadorDelRegistro;

        // Respuesta 403 con el mismo formato ResponseAPI que el resto de la API
        public static ObjectResult Prohibido(this ControllerBase Controlador, string Mensaje) =>
            Controlador.StatusCode(StatusCodes.Status403Forbidden,
                new ResponseAPI<int> { EsCorrecto = false, Mensaje = Mensaje });
    }
}
