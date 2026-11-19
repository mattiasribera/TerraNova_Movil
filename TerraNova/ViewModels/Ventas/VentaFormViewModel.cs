using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Helpers;
using TerraNova.Services;
using TerraNova.Shared;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Ventas/RegistrarVenta.razor (con calculo automatico de comision)
    public partial class VentaFormViewModel : FormularioViewModel
    {
        private readonly ServicioVentas Servicio;
        private readonly ServicioClientes ServicioClientes;
        private readonly ServicioPropiedades ServicioPropiedades;
        private readonly ServicioTrabajadores ServicioTrabajadores;
        private VentaDTO Venta = new() { Fecha = DateTime.Now, FormaPago = "Contado", Estado = "Completada" };

        public VentaFormViewModel(ServicioVentas servicio, ServicioClientes servicioClientes,
                                  ServicioPropiedades servicioPropiedades, ServicioTrabajadores servicioTrabajadores,
                                  ServicioAutenticacion autenticacion)
        {
            Servicio = servicio;
            ServicioClientes = servicioClientes;
            ServicioPropiedades = servicioPropiedades;
            ServicioTrabajadores = servicioTrabajadores;
            Autenticacion = autenticacion;
            Titulo = "Registrar venta";
        }

        private readonly ServicioAutenticacion Autenticacion;

        // Solo el Admin puede elegir el agente; un agente registra las ventas a su nombre
        public bool EsAdmin => Autenticacion.EsAdmin;

        public List<string> FormasPago { get; } = new() { "Contado", "Crédito", "Financiado", "Alquiler" };
        public List<string> Estados { get; } = new() { "Completada", "Pendiente", "Cancelada" };
        public ObservableCollection<ClienteDTO> Clientes { get; } = new();
        public ObservableCollection<PropiedadDTO> Propiedades { get; } = new();
        public ObservableCollection<TrabajadorDTO> Trabajadores { get; } = new();

        [ObservableProperty] public partial ClienteDTO? Cliente { get; set; }
        [ObservableProperty] public partial PropiedadDTO? Propiedad { get; set; }
        [ObservableProperty] public partial TrabajadorDTO? Trabajador { get; set; }
        [ObservableProperty] public partial string Monto { get; set; } = string.Empty; // texto: acepta coma o punto
        [ObservableProperty] public partial decimal Comision { get; set; }
        [ObservableProperty] public partial string FormaPago { get; set; } = "Contado";
        [ObservableProperty] public partial string Estado { get; set; } = "Completada";
        [ObservableProperty] public partial DateTime Fecha { get; set; } = DateTime.Today;
        [ObservableProperty] public partial string Observaciones { get; set; } = string.Empty;

        // Al elegir propiedad: el monto toma su precio y se recalcula la comision
        partial void OnPropiedadChanged(PropiedadDTO? value)
        {
            if (value == null) return;
            Monto = Numeros.Texto(value.Precio);
            if (value.TipoOperacion == "Alquiler") FormaPago = "Alquiler";
        }
        partial void OnTrabajadorChanged(TrabajadorDTO? value) => CalcularComision();
        partial void OnMontoChanged(string value) => CalcularComision();

        private void CalcularComision() =>
            Comision = Trabajador == null || !Numeros.TryLeer(Monto, out var ValorMonto) ? 0
                : decimal.Round(ValorMonto * (Trabajador.PorcentajeComision / 100m), 2);

        protected override async Task CargarDatos()
        {
            var TareaClientes = ServicioClientes.Lista();
            var TareaTrabajadores = ServicioTrabajadores.Activos();
            // Al registrar solo se ofrecen propiedades Disponibles; al editar, todas
            var TareaPropiedades = EsEdicion ? ServicioPropiedades.Lista() : ServicioPropiedades.Disponibles();
            await Task.WhenAll(TareaClientes, TareaTrabajadores, TareaPropiedades);

            foreach (var C in TareaClientes.Result) Clientes.Add(C);
            foreach (var T in TareaTrabajadores.Result) Trabajadores.Add(T);
            foreach (var P in TareaPropiedades.Result) Propiedades.Add(P);

            if (EsEdicion)
            {
                Titulo = "Editar venta";
                Venta = await Servicio.Buscar(Id!.Value);
                Cliente = Clientes.FirstOrDefault(C => C.Id == Venta.IdCliente);
                Propiedad = Propiedades.FirstOrDefault(P => P.Id == Venta.IdPropiedad);
                Trabajador = Trabajadores.FirstOrDefault(T => T.Id == Venta.IdTrabajador);
                Monto = Numeros.Texto(Venta.Monto);
                Comision = Venta.ComisionGenerada;
                FormaPago = Venta.FormaPago;
                Estado = Venta.Estado;
                Fecha = Venta.Fecha;
                Observaciones = Venta.Observaciones ?? string.Empty;
            }
            else if (!EsAdmin)
            {
                Trabajador = Trabajadores.FirstOrDefault(T => T.Id == Autenticacion.UsuarioActual?.IdTrabajador);
            }
        }

        [RelayCommand]
        private async Task Guardar()
        {
            if (Cliente == null || Propiedad == null || Trabajador == null)
            {
                await Dialogos.Alerta("Datos incompletos", "Debe seleccionar Cliente, Propiedad y Agente antes de guardar.");
                return;
            }
            if (!Numeros.TryLeer(Monto, out var ValorMonto) || ValorMonto <= 0)
            {
                await Dialogos.Alerta("Datos incompletos", "El monto debe ser mayor a 0.");
                return;
            }

            Venta.IdCliente = Cliente.Id;
            Venta.IdPropiedad = Propiedad.Id;
            Venta.IdTrabajador = EsAdmin ? Trabajador.Id : Autenticacion.UsuarioActual?.IdTrabajador ?? Trabajador.Id;
            Venta.Monto = ValorMonto;
            Venta.ComisionGenerada = Comision;
            Venta.FormaPago = FormaPago;
            Venta.Estado = Estado;
            Venta.Fecha = Fecha;
            Venta.Observaciones = Observaciones;
            Venta.IdClienteNavigation = null;
            Venta.IdPropiedadNavigation = null;
            Venta.IdTrabajadorNavigation = null;

            await GuardarYVolver(() => EsEdicion ? Servicio.Modificar(Venta) : Servicio.Guardar(Venta), "Venta");
        }
    }
}
