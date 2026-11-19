using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Services;
using TerraNova.Shared;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Visitas/RegistrarVisita.razor. La API envia un email al cliente al guardar.
    public partial class VisitaFormViewModel : FormularioViewModel
    {
        private readonly ServicioVisitas Servicio;
        private readonly ServicioClientes ServicioClientes;
        private readonly ServicioPropiedades ServicioPropiedades;
        private readonly ServicioTrabajadores ServicioTrabajadores;
        private readonly ServicioAutenticacion Autenticacion;
        private VisitaDTO Visita = new() { Estado = "Programada" };
        private int? IdPropiedadInicial;

        public VisitaFormViewModel(ServicioVisitas servicio, ServicioClientes servicioClientes, ServicioPropiedades servicioPropiedades,
                                   ServicioTrabajadores servicioTrabajadores, ServicioAutenticacion autenticacion)
        {
            Servicio = servicio;
            ServicioClientes = servicioClientes;
            ServicioPropiedades = servicioPropiedades;
            ServicioTrabajadores = servicioTrabajadores;
            Autenticacion = autenticacion;
            Titulo = "Programar visita";
        }

        public List<string> Estados { get; } = new() { "Programada", "Realizada", "Cancelada" };

        // Solo el Admin puede elegir el agente; un agente queda asignado automaticamente
        public bool EsAdmin => Autenticacion.EsAdmin;
        public ObservableCollection<ClienteDTO> Clientes { get; } = new();
        public ObservableCollection<PropiedadDTO> Propiedades { get; } = new();
        public ObservableCollection<TrabajadorDTO> Trabajadores { get; } = new();

        [ObservableProperty] public partial ClienteDTO? Cliente { get; set; }
        [ObservableProperty] public partial PropiedadDTO? Propiedad { get; set; }
        [ObservableProperty] public partial TrabajadorDTO? Trabajador { get; set; }
        [ObservableProperty] public partial DateTime Fecha { get; set; } = DateTime.Today.AddDays(1);
        [ObservableProperty] public partial TimeSpan Hora { get; set; } = new(10, 0, 0);
        [ObservableProperty] public partial string Estado { get; set; } = "Programada";
        [ObservableProperty] public partial string Comentarios { get; set; } = string.Empty;

        // Desde el detalle de una propiedad se llega con ?IdPropiedad= para dejarla preseleccionada
        public override void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            base.ApplyQueryAttributes(query);
            if (query.TryGetValue("IdPropiedad", out var Valor) && int.TryParse(Valor?.ToString(), out var Cod))
                IdPropiedadInicial = Cod;
        }

        protected override async Task CargarDatos()
        {
            var TareaClientes = ServicioClientes.Lista();
            var TareaPropiedades = ServicioPropiedades.Lista();
            var TareaTrabajadores = ServicioTrabajadores.Activos();
            await Task.WhenAll(TareaClientes, TareaPropiedades, TareaTrabajadores);

            foreach (var C in TareaClientes.Result) Clientes.Add(C);
            foreach (var P in TareaPropiedades.Result) Propiedades.Add(P);
            foreach (var T in TareaTrabajadores.Result) Trabajadores.Add(T);

            if (EsEdicion)
            {
                Titulo = "Editar visita";
                Visita = await Servicio.Buscar(Id!.Value);
                Cliente = Clientes.FirstOrDefault(C => C.Id == Visita.IdCliente);
                Propiedad = Propiedades.FirstOrDefault(P => P.Id == Visita.IdPropiedad);
                Trabajador = Trabajadores.FirstOrDefault(T => T.Id == Visita.IdTrabajador);
                Fecha = Visita.FechaVisita.Date;
                Hora = Visita.FechaVisita.TimeOfDay;
                Estado = Visita.Estado;
                Comentarios = Visita.Comentarios ?? string.Empty;
            }
            else
            {
                if (IdPropiedadInicial.HasValue)
                    Propiedad = Propiedades.FirstOrDefault(P => P.Id == IdPropiedadInicial.Value);
                // Admin: por defecto el agente de la propiedad. Agente: siempre el mismo (ver Guardar)
                Trabajador = Trabajadores.FirstOrDefault(T => T.Id == (EsAdmin ? Propiedad?.IdTrabajador : Autenticacion.UsuarioActual?.IdTrabajador));
            }
        }

        [RelayCommand]
        private async Task Guardar()
        {
            if (Cliente == null || Propiedad == null || Trabajador == null)
            {
                await Dialogos.Alerta("Datos incompletos", "Debe seleccionar Cliente, Propiedad y Agente.");
                return;
            }

            // Una visita "Programada" tiene que ser a futuro. Las Realizadas/Canceladas pueden ser pasadas
            // (se esta registrando algo que ya ocurrio). La API valida la misma regla.
            if (Estado == "Programada" && Fecha.Date + Hora < DateTime.Now)
            {
                await Dialogos.Alerta("Fecha inválida",
                    "Una visita programada debe ser para una fecha y hora futura.\n" +
                    "Si la visita ya ocurrió, cambie el estado a \"Realizada\" o \"Cancelada\".");
                return;
            }

            Visita.IdCliente = Cliente.Id;
            Visita.IdPropiedad = Propiedad.Id;
            // Un agente siempre agenda las visitas a su propio nombre
            Visita.IdTrabajador = EsAdmin ? Trabajador.Id : Autenticacion.UsuarioActual?.IdTrabajador ?? Trabajador.Id;
            Visita.FechaVisita = Fecha.Date + Hora;
            Visita.Estado = Estado;
            Visita.Comentarios = Comentarios;
            Visita.IdClienteNavigation = null;
            Visita.IdPropiedadNavigation = null;
            Visita.IdTrabajadorNavigation = null;

            await GuardarYVolver(() => EsEdicion ? Servicio.Modificar(Visita) : Servicio.Guardar(Visita), "Visita");
        }
    }
}
