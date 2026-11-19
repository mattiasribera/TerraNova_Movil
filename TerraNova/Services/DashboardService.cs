using TerraNova.Constantes;
using TerraNova.Models;
using TerraNova.Shared;

namespace TerraNova.Services
{
    // Misma logica que Blazor.Client/Services/DashboardService.cs
    public class DashboardService
    {
        private readonly ServicioPropiedades Propiedades;
        private readonly ServicioClientes Clientes;
        private readonly ServicioVentas Ventas;
        private readonly ServicioTrabajadores Trabajadores;
        private readonly ServicioVisitas Visitas;

        public DashboardService(ServicioPropiedades propiedades, ServicioClientes clientes, ServicioVentas ventas,
                                ServicioTrabajadores trabajadores, ServicioVisitas visitas)
        {
            Propiedades = propiedades;
            Clientes = clientes;
            Ventas = ventas;
            Trabajadores = trabajadores;
            Visitas = visitas;
        }

        public async Task<ResumenDashboard> ObtenerDashboardAsync()
        {
            // Se piden todos los datos en paralelo para que cargue mas rapido
            var TareaPropiedades = Propiedades.Lista();
            var TareaClientes = Clientes.Lista();
            var TareaVentas = Ventas.Lista();
            var TareaTrabajadores = Trabajadores.Lista();
            var TareaVisitas = Visitas.Lista();
            await Task.WhenAll(TareaPropiedades, TareaClientes, TareaVentas, TareaTrabajadores, TareaVisitas);

            var ListaPropiedades = TareaPropiedades.Result;
            var ListaVentas = TareaVentas.Result;
            var ListaVisitas = TareaVisitas.Result;

            var Actividades = new List<ActividadReciente>();
            Actividades.AddRange(ListaPropiedades.Select(P => new ActividadReciente
            {
                Descripcion = $"Nueva propiedad registrada: {P.Titulo}",
                Icono = Iconos.Propiedades,
                Fecha = P.FechaPublicacion
            }));
            Actividades.AddRange(ListaVentas.Select(V => new ActividadReciente
            {
                Descripcion = $"Nueva transacción ({(V.FormaPago == "Alquiler" ? "Alquiler" : "Venta")}) registrada",
                Icono = Iconos.Ventas,
                Fecha = V.Fecha
            }));
            Actividades.AddRange(ListaVisitas.Select(V => new ActividadReciente
            {
                Descripcion = $"Visita {V.Estado.ToLower()} para {V.IdPropiedadNavigation?.Titulo ?? $"la propiedad ID: {V.IdPropiedad}"}",
                Icono = Iconos.Visitas,
                Fecha = V.FechaVisita
            }));

            return new ResumenDashboard
            {
                TotalPropiedades = ListaPropiedades.Count,
                TotalClientes = TareaClientes.Result.Count,
                TotalVentas = ListaVentas.Count,
                TotalTrabajadores = TareaTrabajadores.Result.Count,
                UltimasPropiedades = ListaPropiedades.OrderByDescending(P => P.FechaPublicacion).Take(5).ToList(),
                ActividadReciente = Actividades.OrderByDescending(A => A.Fecha).Take(5).ToList()
            };
        }
    }
}
