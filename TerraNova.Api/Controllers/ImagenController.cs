using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerraNova.Api.Services;
using TerraNova.Shared;

namespace TerraNova.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImagenController : ControllerBase
    {
        private readonly ServicioImagenes Imagenes;

        public ImagenController(ServicioImagenes imagenes)
        {
            Imagenes = imagenes;
        }

        // POST api/Imagen/Subir/trabajadores   (multipart/form-data, campo "archivo")
        // Devuelve la ruta que se guarda en FotoUrl / ImagenUrl
        [HttpPost]
        [Route("Subir/{Carpeta}")]
        [RequestSizeLimit(ServicioImagenes.TamanoMaximo + 64 * 1024)]
        public async Task<IActionResult> Subir(string Carpeta, IFormFile archivo)
        {
            var RespuestaApi = new ResponseAPI<string>();
            try
            {
                RespuestaApi.Valor = await Imagenes.Guardar(archivo, Carpeta);
                RespuestaApi.EsCorrecto = true;
                RespuestaApi.Mensaje = "Imagen subida correctamente";
            }
            catch (Exception ex)
            {
                RespuestaApi.EsCorrecto = false;
                RespuestaApi.Mensaje = ex.Message;
            }
            return Ok(RespuestaApi);
        }
    }
}
