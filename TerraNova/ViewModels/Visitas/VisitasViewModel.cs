using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Services;
using TerraNova.Shared;
using TerraNova.Constantes;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Visitas/ListarVisitas.razor (busqueda + filtro por estado y rango de fechas)
    public partial class VisitasViewModel : ObservableObject, ICargable
    {
        private readonly ServicioVisitas Servicio;
        private List<VisitaDTO> Todas = new();

        public VisitasViewModel(ServicioVisitas servicio)
        {
            Servicio = servicio;
        }

        public ObservableCollection<VisitaDTO> Visitas { get; } = new();
        private const string TodosLosEstados = "Todos los estados";

        public List<string> Estados { get; } = new() { TodosLosEstados, "Programada", "Realizada", "Cancelada" };

        [ObservableProperty] public partial string TextoBusqueda { get; set; } = string.Empty;
        [ObservableProperty] public partial string EstadoFiltro { get; set; } = TodosLosEstados;
        [ObservableProperty] public partial bool FiltrarFechas { get; set; }
        [ObservableProperty] public partial DateTime FechaDesde { get; set; } = DateTime.Today.AddMonths(-1);
        [ObservableProperty] public partial DateTime FechaHasta { get; set; } = DateTime.Today.AddMonths(1);
        [ObservableProperty] public partial bool Cargando { get; set; }

        partial void OnTextoBusquedaChanged(string value) => Filtrar();
        partial void OnEstadoFiltroChanged(string value) => Filtrar();
        partial void OnFiltrarFechasChanged(bool value) => Filtrar();
        partial void OnFechaDesdeChanged(DateTime value) => Filtrar();
        partial void OnFechaHastaChanged(DateTime value) => Filtrar();

        [RelayCommand]
        public async Task Cargar()
        {
            try
            {
                Cargando = true;
                Todas = (await Servicio.Lista()).OrderByDescending(V => V.FechaVisita).ToList();
                Filtrar();
            }
            catch (Exception Ex) { await Dialogos.Alerta("Error", Ex.Message); }
            finally { Cargando = false; }
        }

        private void Filtrar()
        {
            IEnumerable<VisitaDTO> R = Todas;
            if (EstadoFiltro != TodosLosEstados)
                R = R.Where(V => V.Estado == EstadoFiltro);
            if (FiltrarFechas)
                R = R.Where(V => V.FechaVisita >= FechaDesde && V.FechaVisita <= FechaHasta.AddDays(1));
            var Texto = TextoBusqueda.Trim().ToLower();
            if (Texto != "")
                R = R.Where(V =>
                    (V.IdClienteNavigation?.Nombre?.ToLower().Contains(Texto) ?? false) ||
                    (V.IdClienteNavigation?.Apellido?.ToLower().Contains(Texto) ?? false) ||
                    (V.IdPropiedadNavigation?.Titulo?.ToLower().Contains(Texto) ?? false) ||
                    (V.Comentarios?.ToLower().Contains(Texto) ?? false));

            Visitas.Clear();
            foreach (var V in R) Visitas.Add(V);
        }

        [RelayCommand]
        private void LimpiarFiltros()
        {
            TextoBusqueda = string.Empty;
            EstadoFiltro = TodosLosEstados;
            FiltrarFechas = false;
        }

        [RelayCommand] private Task Nueva() => Shell.Current.GoToAsync(Rutas.VisitaForm);
        [RelayCommand] private Task Editar(VisitaDTO V) => Shell.Current.GoToAsync(Rutas.ConId(Rutas.VisitaForm, V.Id));

        [RelayCommand]
        private async Task Eliminar(VisitaDTO V)
        {
            if (!await Dialogos.Confirmar("Eliminar", "¿Eliminar esta visita?")) return;
            try
            {
                await Servicio.Eliminar(V.Id);
                Todas.Remove(V);
                Visitas.Remove(V);
                await Dialogos.Aviso("Visita eliminada");
            }
            catch (Exception Ex) { await Dialogos.Alerta("No se pudo eliminar", Ex.Message); }
        }
    }
}
