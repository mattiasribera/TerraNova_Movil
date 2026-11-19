using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Services;
using TerraNova.Shared;
using TerraNova.Constantes;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Ventas/ListarVentas.razor
    public partial class VentasViewModel : ObservableObject, ICargable
    {
        private readonly ServicioVentas Servicio;
        private List<VentaDTO> Todas = new();

        public VentasViewModel(ServicioVentas servicio)
        {
            Servicio = servicio;
        }

        public ObservableCollection<VentaDTO> Ventas { get; } = new();

        [ObservableProperty] public partial string TextoBusqueda { get; set; } = string.Empty;
        [ObservableProperty] public partial bool Cargando { get; set; }
        [ObservableProperty] public partial decimal TotalMontos { get; set; }
        [ObservableProperty] public partial decimal TotalComisiones { get; set; }

        partial void OnTextoBusquedaChanged(string value) => Filtrar();

        [RelayCommand]
        public async Task Cargar()
        {
            try
            {
                Cargando = true;
                Todas = (await Servicio.Lista()).OrderByDescending(V => V.Fecha).ToList();
                Filtrar();
            }
            catch (Exception Ex) { await Dialogos.Alerta("Error", Ex.Message); }
            finally { Cargando = false; }
        }

        private void Filtrar()
        {
            var Texto = TextoBusqueda.Trim().ToLower();
            var R = Todas.Where(V => Texto == "" ||
                (V.IdClienteNavigation?.Nombre?.ToLower().Contains(Texto) ?? false) ||
                (V.IdClienteNavigation?.Apellido?.ToLower().Contains(Texto) ?? false) ||
                (V.IdPropiedadNavigation?.Titulo?.ToLower().Contains(Texto) ?? false) ||
                (V.IdTrabajadorNavigation?.Nombre?.ToLower().Contains(Texto) ?? false) ||
                (V.IdTrabajadorNavigation?.Apellido?.ToLower().Contains(Texto) ?? false)).ToList();

            Ventas.Clear();
            foreach (var V in R) Ventas.Add(V);
            TotalMontos = R.Sum(V => V.Monto);
            TotalComisiones = R.Sum(V => V.ComisionGenerada);
        }

        [RelayCommand] private Task Nueva() => Shell.Current.GoToAsync(Rutas.VentaForm);
        [RelayCommand] private Task Editar(VentaDTO V) => Shell.Current.GoToAsync(Rutas.ConId(Rutas.VentaForm, V.Id));

        [RelayCommand]
        private async Task Eliminar(VentaDTO V)
        {
            if (!await Dialogos.Confirmar("Anular venta", "¿Anular esta venta? La propiedad volverá a estar Disponible.")) return;
            try
            {
                await Servicio.Eliminar(V.Id);
                await Dialogos.Aviso("Venta anulada");
                await Cargar();
            }
            catch (Exception Ex) { await Dialogos.Alerta("No se pudo anular", Ex.Message); }
        }
    }
}
