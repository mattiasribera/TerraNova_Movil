using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerraNova.Api.Models;
using TerraNova.Api.Services;
using TerraNova.Shared;

namespace TerraNova.Api.Controllers
{
    // Solo para desarrollo/presentacion: llena la base con datos de demostracion.
    // POST api/Demo/Generar   (desde Swagger: http://localhost:5150/swagger)
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class DemoController : ControllerBase
    {
        private readonly TerraNovaDbContext Contexto;
        private readonly IWebHostEnvironment Entorno;

        public DemoController(TerraNovaDbContext contexto, IWebHostEnvironment entorno)
        {
            Contexto = contexto;
            Entorno = entorno;
        }

        [HttpPost]
        [Route("Generar")]
        public IActionResult Generar(bool completarFotos = true)
        {
            var Respuesta = new ResponseAPI<GeneradorDemo.Resumen>();
            if (!Entorno.IsDevelopment())
            {
                Respuesta.Mensaje = "Solo disponible en desarrollo";
                return Ok(Respuesta);
            }
            try
            {
                Respuesta.Valor = GeneradorDemo.Generar(Contexto, completarFotos);
                Respuesta.EsCorrecto = true;
                Respuesta.Mensaje = Respuesta.Valor.YaExistia
                    ? "Los datos de demostración ya estaban cargados"
                    : $"Datos generados. Usuarios agente: contraseña {GeneradorDemo.ClaveAgentes}";
            }
            catch (Exception ex)
            {
                Respuesta.Mensaje = $"{ex.Message} | {ex.InnerException?.Message}";
            }
            return Ok(Respuesta);
        }
    }
}
