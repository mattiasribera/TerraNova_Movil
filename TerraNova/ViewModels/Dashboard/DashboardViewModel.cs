using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Models;
using TerraNova.Services;
using TerraNova.Shared;
using TerraNova.Constantes;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Home.razor
    public partial class DashboardViewModel : ObservableObject, ICargable
    {
        private readonly DashboardService Servicio;
        private readonly ServicioAutenticacion Autenticacion;

        public DashboardViewModel(DashboardService servicio, ServicioAutenticacion autenticacion)
        {
            Servicio = servicio;
            Autenticacion = autenticacion;
        }

        public string Saludo => $"Hola, {Autenticacion.UsuarioActual?.NombreUsuario}";

        [ObservableProperty]
        public partial ResumenDashboard? Datos { get; set; }

        [ObservableProperty]
        public partial bool Cargando { get; set; }

        [RelayCommand]
        public async Task Cargar()
        {
            try
            {
                Cargando = true;
                OnPropertyChanged(nameof(Saludo));
                // Un agente ve sus propias estadisticas; el Admin, las de toda la empresa
                var IdAgente = Autenticacion.EsAdmin ? null : Autenticacion.UsuarioActual?.IdTrabajador;
                Datos = await Servicio.ObtenerDashboardAsync(IdAgente);
            }
            catch (Exception Ex)
            {
                await Dialogos.Alerta("Error", "No se pudo cargar el inicio: " + Ex.Message);
            }
            finally
            {
                Cargando = false;
            }
        }

        // Accesos rapidos (igual que los botones del Home web)
        [RelayCommand]
        private Task Ir(string Ruta) => Shell.Current.GoToAsync(Ruta);

        [RelayCommand]
        private Task VerPropiedad(PropiedadDTO P) => Shell.Current.GoToAsync(Rutas.ConId(Rutas.DetallePropiedad, P.Id));

        [RelayCommand]
        private Task VerVisita(VisitaDTO V) => Shell.Current.GoToAsync(Rutas.ConId(Rutas.VisitaForm, V.Id));
    }
}
