using Microsoft.AspNetCore.Authorization;
using TerraNova.Api.Services;
using Microsoft.AspNetCore.Mvc;
using TerraNova.Api.Models;
using TerraNova.Shared;
using Microsoft.EntityFrameworkCore;

namespace TerraNova.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VisitaController : ControllerBase
    {
        private readonly TerraNovaDbContext Contexto;
        private readonly ServicioEmail _servicioEmail;

        public VisitaController(TerraNovaDbContext contexto, ServicioEmail servicioEmail)
        {
            Contexto = contexto;
            _servicioEmail = servicioEmail;
        }

        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {
            var EsAdminActual = User.EsAdmin();
            var IdMio = User.IdTrabajador();
            var RespuestaApi = new ResponseAPI<List<VisitaDTO>>();
            var ListaVisitas = new List<VisitaDTO>();
            try
            {
                var visitas = await Contexto.Visitas.Where(x => EsAdminActual || x.IdTrabajador == IdMio)
                    .Include(v => v.IdClienteNavigation)
                    .Include(v => v.IdPropiedadNavigation)
                    .Include(v => v.IdTrabajadorNavigation)
                    .ToListAsync();

                foreach (var Visita in visitas)
                {
                    ListaVisitas.Add(new VisitaDTO
                    {
                        Id = Visita.Id,
                        IdCliente = Visita.IdCliente,
                        IdPropiedad = Visita.IdPropiedad,
                        IdTrabajador = Visita.IdTrabajador,
                        FechaVisita = Visita.FechaVisita,
                        Estado = Visita.Estado,
                        Comentarios = Visita.Comentarios,
                        IdClienteNavigation = new ClienteDTO
                        {
                            Id = Visita.IdClienteNavigation.Id,
                            Nombre = Visita.IdClienteNavigation.Nombre,
                            Apellido = Visita.IdClienteNavigation.Apellido
                        },
                        IdPropiedadNavigation = new PropiedadDTO
                        {
                            Id = Visita.IdPropiedadNavigation.Id,
                            Titulo = Visita.IdPropiedadNavigation.Titulo
                        },
                        IdTrabajadorNavigation = new TrabajadorDTO
                        {
                            Id = Visita.IdTrabajadorNavigation.Id,
                            Nombre = Visita.IdTrabajadorNavigation.Nombre,
                            Apellido = Visita.IdTrabajadorNavigation.Apellido
                        }
                    });
                }
                RespuestaApi.EsCorrecto = true;
                RespuestaApi.Valor = ListaVisitas;
                RespuestaApi.Mensaje = "Lista de Visitas Preparada";
            }
            catch (Exception ex)
            {
                RespuestaApi.EsCorrecto = false;
                RespuestaApi.Mensaje = $"{ex.Message} | {ex.InnerException?.Message}";
            }
            return Ok(RespuestaApi);
        }

        [HttpGet]
        [Route("Proximas")]
        public async Task<IActionResult> Proximas()
        {
            var EsAdminActual = User.EsAdmin();
            var IdMio = User.IdTrabajador();
            var RespuestaApi = new ResponseAPI<List<VisitaDTO>>();
            try
            {
                var visitas = await Contexto.Visitas.Where(x => EsAdminActual || x.IdTrabajador == IdMio)
                    .Where(v => v.Estado == "Programada")
                    .OrderBy(v => v.FechaVisita)
                    .Select(v => new VisitaDTO
                    {
                        Id = v.Id,
                        IdCliente = v.IdCliente,
                        IdPropiedad = v.IdPropiedad,
                        IdTrabajador = v.IdTrabajador,
                        FechaVisita = v.FechaVisita,
                        Estado = v.Estado,
                        Comentarios = v.Comentarios
                    })
                    .ToListAsync();

                RespuestaApi.EsCorrecto = true;
                RespuestaApi.Valor = visitas;
                RespuestaApi.Mensaje = "Visitas próximas preparadas";
            }
            catch (Exception ex)
            {
                RespuestaApi.EsCorrecto = false;
                RespuestaApi.Mensaje = $"{ex.Message} | {ex.InnerException?.Message}";
            }
            return Ok(RespuestaApi);
        }

        [HttpPost]
        [Route("Guardar")]
        public async Task<IActionResult> Guardar(VisitaDTO ObjVisita)
        {
            if (!User.EsAdmin())
            {
                if (User.IdTrabajador() is not int Mio) return this.Prohibido("Su usuario no tiene un trabajador vinculado");
                ObjVisita.IdTrabajador = Mio; // un agente siempre registra a su nombre
            }

            var RespuestaAPI = new ResponseAPI<int>();
            try
            {
                var DatosVisita = new Visita
                {
                    IdCliente = ObjVisita.IdCliente,
                    IdPropiedad = ObjVisita.IdPropiedad,
                    IdTrabajador = ObjVisita.IdTrabajador,
                    FechaVisita = ObjVisita.FechaVisita,
                    Estado = ObjVisita.Estado,
                    Comentarios = ObjVisita.Comentarios
                };
                Contexto.Visitas.Add(DatosVisita);
                await Contexto.SaveChangesAsync();
                if (DatosVisita.Id != 0)
                {
                    RespuestaAPI.EsCorrecto = true;
                    RespuestaAPI.Valor = DatosVisita.Id;
                    RespuestaAPI.Mensaje = "Visita programada correctamente";

                    try
                    {
                        await _servicioEmail.EnviarNotificacionVisita(DatosVisita.Id);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    RespuestaAPI.EsCorrecto = false;
                    RespuestaAPI.Mensaje = "Datos no guardados";
                }
            }
            catch (Exception ex)
            {
                RespuestaAPI.EsCorrecto = false;
                RespuestaAPI.Mensaje = $"{ex.Message} | {ex.InnerException?.Message}";
            }
            return Ok(RespuestaAPI);
        }

        [HttpDelete]
        [Route("Eliminar/{Cod}")]
        public async Task<IActionResult> Eliminar(int Cod)
        {
            if (!User.EsAdmin())
            {
                var Duenio = await Contexto.Visitas.Where(x => x.Id == Cod).Select(x => (int?)x.IdTrabajador).FirstOrDefaultAsync();
                if (Duenio == null || !User.PuedeGestionar(Duenio.Value)) return this.Prohibido("Solo puede eliminar sus propias visitas");
            }

            var RespuestaApi = new ResponseAPI<int>();
            try
            {
                var VisitaEliminar = await Contexto.Visitas.FirstOrDefaultAsync(v => v.Id == Cod);
                if (VisitaEliminar != null)
                {
                    Contexto.Visitas.Remove(VisitaEliminar);
                    await Contexto.SaveChangesAsync();
                    RespuestaApi.EsCorrecto = true;
                    RespuestaApi.Mensaje = "Visita eliminada";
                }
                else
                {
                    RespuestaApi.EsCorrecto = false;
                    RespuestaApi.Mensaje = "Visita no encontrada";
                }
            }
            catch (Exception ex)
            {
                RespuestaApi.EsCorrecto = false;
                RespuestaApi.Mensaje = $"{ex.Message} | {ex.InnerException?.Message}";
            }
            return Ok(RespuestaApi);
        }

        [HttpGet]
        [Route("Buscar/{Cod}")]
        public async Task<IActionResult> Buscar(int Cod)
        {
            if (!User.EsAdmin())
            {
                var Duenio = await Contexto.Visitas.Where(x => x.Id == Cod).Select(x => (int?)x.IdTrabajador).FirstOrDefaultAsync();
                if (Duenio == null || !User.PuedeGestionar(Duenio.Value)) return this.Prohibido("Solo puede ver sus propias visitas");
            }

            var RespuestaApi = new ResponseAPI<VisitaDTO>();
            var VisitaBuscada = new VisitaDTO();
            try
            {
                var VisitaBd = await Contexto.Visitas.FirstOrDefaultAsync(v => v.Id == Cod);
                if (VisitaBd != null)
                {
                    VisitaBuscada.Id = VisitaBd.Id;
                    VisitaBuscada.IdCliente = VisitaBd.IdCliente;
                    VisitaBuscada.IdPropiedad = VisitaBd.IdPropiedad;
                    VisitaBuscada.IdTrabajador = VisitaBd.IdTrabajador;
                    VisitaBuscada.FechaVisita = VisitaBd.FechaVisita;
                    VisitaBuscada.Estado = VisitaBd.Estado;
                    VisitaBuscada.Comentarios = VisitaBd.Comentarios;

                    RespuestaApi.EsCorrecto = true;
                    RespuestaApi.Valor = VisitaBuscada;
                }
                else
                {
                    RespuestaApi.EsCorrecto = false;
                    RespuestaApi.Mensaje = "Visita No Encontrada";
                }
            }
            catch (Exception ex)
            {
                RespuestaApi.EsCorrecto = false;
                RespuestaApi.Mensaje = $"{ex.Message} | {ex.InnerException?.Message}";
            }
            return Ok(RespuestaApi);
        }

        [HttpPut]
        [Route("Modificar/{Cod}")]
        public async Task<IActionResult> Modificar(VisitaDTO NuevosDatos, int Cod)
        {
            if (!User.EsAdmin())
            {
                var Duenio = await Contexto.Visitas.Where(x => x.Id == Cod).Select(x => (int?)x.IdTrabajador).FirstOrDefaultAsync();
                if (Duenio == null || !User.PuedeGestionar(Duenio.Value)) return this.Prohibido("Solo puede modificar sus propias visitas");
                NuevosDatos.IdTrabajador = Duenio.Value;
            }

            var RespuestaApi = new ResponseAPI<int>();
            try
            {
                var VisitaBd = await Contexto.Visitas.FirstOrDefaultAsync(v => v.Id == Cod);

                if (VisitaBd != null)
                {
                    VisitaBd.IdCliente = NuevosDatos.IdCliente;
                    VisitaBd.IdPropiedad = NuevosDatos.IdPropiedad;
                    VisitaBd.IdTrabajador = NuevosDatos.IdTrabajador;
                    VisitaBd.FechaVisita = NuevosDatos.FechaVisita;
                    VisitaBd.Estado = NuevosDatos.Estado;
                    VisitaBd.Comentarios = NuevosDatos.Comentarios;

                    Contexto.Visitas.Update(VisitaBd);
                    await Contexto.SaveChangesAsync();

                    RespuestaApi.EsCorrecto = true;
                    RespuestaApi.Valor = VisitaBd.Id;

                    try
                    {
                        await _servicioEmail.EnviarNotificacionVisita(VisitaBd.Id);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    RespuestaApi.EsCorrecto = false;
                    RespuestaApi.Mensaje = "Visita no encontrada";
                }
            }
            catch (Exception ex)
            {
                RespuestaApi.EsCorrecto = false;
                RespuestaApi.Mensaje = $"{ex.Message} | {ex.InnerException?.Message}";
            }
            return Ok(RespuestaApi);
        }

        [HttpGet]
        [Route("PorPropiedad/{IdPropiedad}")]
        public async Task<IActionResult> PorPropiedad(int IdPropiedad)
        {
            var EsAdminActual = User.EsAdmin();
            var IdMio = User.IdTrabajador();
            var RespuestaApi = new ResponseAPI<List<VisitaDTO>>();
            try
            {
                var visitas = await Contexto.Visitas.Where(x => EsAdminActual || x.IdTrabajador == IdMio)
                    .Where(v => v.IdPropiedad == IdPropiedad)
                    .Select(v => new VisitaDTO
                    {
                        Id = v.Id,
                        IdCliente = v.IdCliente,
                        IdPropiedad = v.IdPropiedad,
                        IdTrabajador = v.IdTrabajador,
                        FechaVisita = v.FechaVisita,
                        Estado = v.Estado,
                        Comentarios = v.Comentarios,
                        // Nombre del cliente para mostrarlo en el detalle de la propiedad
                        IdClienteNavigation = new ClienteDTO
                        {
                            Id = v.IdClienteNavigation.Id,
                            Nombre = v.IdClienteNavigation.Nombre,
                            Apellido = v.IdClienteNavigation.Apellido
                        }
                    })
                    .OrderByDescending(v => v.FechaVisita)
                    .ToListAsync();

                RespuestaApi.EsCorrecto = true;
                RespuestaApi.Valor = visitas;
                RespuestaApi.Mensaje = "Visitas de la propiedad preparadas";
            }
            catch (Exception ex)
            {
                RespuestaApi.EsCorrecto = false;
                RespuestaApi.Mensaje = $"{ex.Message} | {ex.InnerException?.Message}";
            }
            return Ok(RespuestaApi);
        }
    }
}