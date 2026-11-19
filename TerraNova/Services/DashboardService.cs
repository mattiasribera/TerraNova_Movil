using System.Globalization;
using TerraNova.Constantes;
using TerraNova.Helpers;
using TerraNova.Models;
using TerraNova.Shared;

namespace TerraNova.Services
{
    // Basado en Blazor.Client/Services/DashboardService.cs, ampliado con graficos (ECharts)
    // y una vista propia para los agentes ("mis estadisticas").
    public class DashboardService
    {
        private static readonly CultureInfo Cultura = new("es-BO");

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

        // IdAgente = null -> vista de Admin (toda la empresa); con valor -> solo los datos de ese agente
        public async Task<ResumenDashboard> ObtenerDashboardAsync(int? IdAgente)
        {
            // Se piden todos los datos en paralelo para que cargue mas rapido
            var TareaPropiedades = Propiedades.Lista();
            var TareaClientes = Clientes.Lista();
            var TareaVentas = Ventas.Lista();
            var TareaTrabajadores = Trabajadores.Lista();
            var TareaVisitas = Visitas.Lista();
            await Task.WhenAll(TareaPropiedades, TareaClientes, TareaVentas, TareaTrabajadores, TareaVisitas);

            var EsAgente = IdAgente.HasValue;
            var ListaPropiedades = TareaPropiedades.Result.Where(P => !EsAgente || P.IdTrabajador == IdAgente).ToList();
            var ListaVentas = TareaVentas.Result.Where(V => !EsAgente || V.IdTrabajador == IdAgente).ToList();
            var ListaVisitas = TareaVisitas.Result.Where(V => !EsAgente || V.IdTrabajador == IdAgente).ToList();

            var Ahora = DateTime.Now;
            var InicioMes = new DateTime(Ahora.Year, Ahora.Month, 1);
            var Proximas = ListaVisitas.Where(V => V.Estado == "Programada" && V.FechaVisita >= Ahora)
                                       .OrderBy(V => V.FechaVisita).ToList();

            var Resumen = new ResumenDashboard
            {
                EsAgente = EsAgente,
                GraficoVentas = OpcionesGraficos.VentasPorMes(ListaVentas),
                UltimasPropiedades = ListaPropiedades.OrderByDescending(P => P.FechaPublicacion).Take(5).ToList(),
                ProximasVisitas = Proximas.Take(6).ToList()
            };

            if (EsAgente)
            {
                // ===== Mis estadisticas =====
                var VentasMes = ListaVentas.Where(V => V.Fecha >= InicioMes).ToList();
                Resumen.Subtitulo = "Tus estadísticas como agente";
                Resumen.Indicadores = new()
                {
                    Nuevo("Mis propiedades", ListaPropiedades.Count.ToString(),
                          $"{ListaPropiedades.Count(P => P.Estado == "Disponible")} disponibles", Iconos.Propiedades, "#1E5631"),
                    Nuevo("Mis ventas", ListaVentas.Count.ToString(),
                          $"{VentasMes.Count} este mes", Iconos.Ventas, "#DC3545"),
                    Nuevo("Comisión del mes", Dinero(VentasMes.Sum(V => V.ComisionGenerada)),
                          $"Total: {Dinero(ListaVentas.Sum(V => V.ComisionGenerada))}", Iconos.Comision, "#B8860B"),
                    Nuevo("Visitas próximas", Proximas.Count.ToString(),
                          $"{Proximas.Count(V => V.FechaVisita.Date == Ahora.Date)} hoy", Iconos.Visitas, "#0D6EFD")
                };
                Resumen.TituloGraficoEstados = "Mis visitas por estado";
                Resumen.GraficoEstados = OpcionesGraficos.PorEstado(ListaVisitas.Select(V => V.Estado));
            }
            else
            {
                // ===== Vista de la empresa (Admin) =====
                Resumen.Subtitulo = "Resumen general de TerraNova Bienes Raíces";
                Resumen.Indicadores = new()
                {
                    Nuevo("Propiedades", ListaPropiedades.Count.ToString(),
                          $"{ListaPropiedades.Count(P => P.Estado == "Disponible")} disponibles", Iconos.Propiedades, "#1E5631"),
                    Nuevo("Clientes", TareaClientes.Result.Count.ToString(),
                          $"{TareaClientes.Result.Count(C => C.FechaRegistro >= InicioMes)} nuevos este mes", Iconos.Clientes, "#0D6EFD"),
                    Nuevo("Ventas", ListaVentas.Count.ToString(),
                          $"{Dinero(ListaVentas.Where(V => V.Fecha >= InicioMes).Sum(V => V.Monto))} este mes", Iconos.Ventas, "#DC3545"),
                    Nuevo("Trabajadores", TareaTrabajadores.Result.Count(T => T.Activo).ToString(),
                          $"{TareaTrabajadores.Result.Count(T => T.Activo && T.Cargo == "Agente")} agentes", Iconos.Trabajadores, "#B8860B")
                };
                Resumen.TituloGraficoEstados = "Propiedades por estado";
                Resumen.GraficoEstados = OpcionesGraficos.PorEstado(ListaPropiedades.Select(P => P.Estado));
                Resumen.GraficoAgentes = OpcionesGraficos.ComisionesPorAgente(ListaVentas);
                Resumen.ActividadReciente = ActividadReciente(ListaPropiedades, ListaVentas, ListaVisitas);
            }
            return Resumen;
        }

        private static List<ActividadReciente> ActividadReciente(List<PropiedadDTO> Props, List<VentaDTO> Ventas, List<VisitaDTO> Visitas)
        {
            var Actividades = new List<ActividadReciente>();
            Actividades.AddRange(Props.Select(P => new ActividadReciente
            {
                Descripcion = $"Nueva propiedad registrada: {P.Titulo}", Icono = Iconos.Propiedades, Fecha = P.FechaPublicacion
            }));
            Actividades.AddRange(Ventas.Select(V => new ActividadReciente
            {
                Descripcion = $"{(V.FormaPago == "Alquiler" ? "Alquiler" : "Venta")} registrada: {V.IdPropiedadNavigation?.Titulo}",
                Icono = Iconos.Ventas, Fecha = V.Fecha
            }));
            Actividades.AddRange(Visitas.Select(V => new ActividadReciente
            {
                Descripcion = $"Visita {V.Estado.ToLower()} en {V.IdPropiedadNavigation?.Titulo ?? $"la propiedad ID: {V.IdPropiedad}"}",
                Icono = Iconos.Visitas, Fecha = V.FechaVisita
            }));
            // Solo lo que ya ocurrio (las visitas programadas a futuro no son "actividad reciente")
            return Actividades.Where(A => A.Fecha <= DateTime.Now).OrderByDescending(A => A.Fecha).Take(6).ToList();
        }

        private static Indicador Nuevo(string Titulo, string Valor, string Detalle, string Icono, string Color) =>
            new() { Titulo = Titulo, Valor = Valor, Detalle = Detalle, Icono = Icono, Color = Microsoft.Maui.Graphics.Color.FromArgb(Color) };

        private static string Dinero(decimal Monto) => "$ " + Monto.ToString("N0", Cultura);
    }
}
