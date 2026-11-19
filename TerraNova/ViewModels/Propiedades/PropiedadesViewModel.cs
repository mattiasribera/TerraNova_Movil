using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Services;
using TerraNova.Shared;
using TerraNova.Constantes;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Propiedades/ListarPropiedades.razor
    public partial class PropiedadesViewModel : ObservableObject, ICargable
    {
        private readonly ServicioPropiedades Servicio;
        private readonly ServicioAutenticacion Autenticacion;
        private List<PropiedadDTO> Todas = new();

        public PropiedadesViewModel(ServicioPropiedades servicio, ServicioAutenticacion autenticacion)
        {
            Servicio = servicio;
            Autenticacion = autenticacion;
        }

        public ObservableCollection<PropiedadDTO> Propiedades { get; } = new();
        private const string TodosLosTipos = "Todos los tipos";
        private const string TodosLosEstados = "Todos los estados";

        public List<string> Tipos { get; } = new() { TodosLosTipos, "Casa", "Departamento", "Terreno", "Oficina", "Local" };
        public List<string> Estados { get; } = new() { TodosLosEstados, "Disponible", "Reservada", "Vendida", "Alquilada" };
        public bool EsAgente => !Autenticacion.EsAdmin;

        [ObservableProperty] public partial string TextoBusqueda { get; set; } = string.Empty;
        [ObservableProperty] public partial string TipoFiltro { get; set; } = TodosLosTipos;
        [ObservableProperty] public partial string EstadoFiltro { get; set; } = TodosLosEstados;
        [ObservableProperty] public partial bool SoloMias { get; set; }
        [ObservableProperty] public partial bool Cargando { get; set; }

        partial void OnTextoBusquedaChanged(string value) => Filtrar();
        partial void OnTipoFiltroChanged(string value) => Filtrar();
        partial void OnEstadoFiltroChanged(string value) => Filtrar();
        partial void OnSoloMiasChanged(bool value) => Filtrar();

        [RelayCommand]
        public async Task Cargar()
        {
            try
            {
                Cargando = true;
                OnPropertyChanged(nameof(EsAgente));
                Todas = await Servicio.Lista();
                Filtrar();
            }
            catch (Exception Ex)
            {
                await Dialogos.Alerta("Error", Ex.Message);
            }
            finally
            {
                Cargando = false;
            }
        }

        private void Filtrar()
        {
            IEnumerable<PropiedadDTO> R = Todas;
            if (SoloMias)
                R = R.Where(P => P.IdTrabajador == Autenticacion.UsuarioActual?.IdTrabajador);
            if (TipoFiltro != TodosLosTipos)
                R = R.Where(P => P.Tipo == TipoFiltro);
            if (EstadoFiltro != TodosLosEstados)
                R = R.Where(P => P.Estado == EstadoFiltro);
            var Texto = TextoBusqueda.Trim().ToLower();
            if (Texto != "")
                R = R.Where(P => P.Titulo.ToLower().Contains(Texto) || P.Zona.ToLower().Contains(Texto) ||
                                 P.Direccion.ToLower().Contains(Texto));

            Propiedades.Clear();
            foreach (var P in R) Propiedades.Add(P);
        }

        [RelayCommand]
        private void LimpiarFiltros()
        {
            TextoBusqueda = string.Empty;
            TipoFiltro = TodosLosTipos;
            EstadoFiltro = TodosLosEstados;
            SoloMias = false;
        }

        private bool PuedeEditar(PropiedadDTO P) =>
            Autenticacion.EsAdmin || P.IdTrabajador == Autenticacion.UsuarioActual?.IdTrabajador;

        [RelayCommand]
        private Task VerDetalle(PropiedadDTO P) => Shell.Current.GoToAsync(Rutas.ConId(Rutas.DetallePropiedad, P.Id));

        [RelayCommand]
        private Task Nueva() => Shell.Current.GoToAsync(Rutas.PropiedadForm);

        [RelayCommand]
        private async Task Editar(PropiedadDTO P)
        {
            if (!PuedeEditar(P))
            {
                await Dialogos.Alerta("Sin permiso", "Solo puede editar las propiedades que tiene asignadas.");
                return;
            }
            await Shell.Current.GoToAsync(Rutas.ConId(Rutas.PropiedadForm, P.Id));
        }

        [RelayCommand]
        private async Task Eliminar(PropiedadDTO P)
        {
            if (!Autenticacion.EsAdmin)
            {
                await Dialogos.Alerta("Sin permiso", "Solo un administrador puede eliminar propiedades.");
                return;
            }
            if (!await Dialogos.Confirmar("Eliminar", $"¿Eliminar '{P.Titulo}'?")) return;
            try
            {
                await Servicio.Eliminar(P.Id);
                Todas.Remove(P);
                Propiedades.Remove(P);
                await Dialogos.Aviso("Propiedad eliminada");
            }
            catch (Exception Ex)
            {
                await Dialogos.Alerta("No se pudo eliminar", Ex.Message);
            }
        }
    }
}
