using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Services;
using TerraNova.Shared;
using TerraNova.Constantes;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Propiedades/DetallePropiedad.razor (datos + mapa + visitas de la propiedad)
    public partial class DetallePropiedadViewModel : ObservableObject, IQueryAttributable, ICargable
    {
        private readonly ServicioPropiedades Servicio;
        private readonly ServicioVisitas ServicioVisitas;
        private readonly ServicioAutenticacion Autenticacion;
        private int Id;

        public DetallePropiedadViewModel(ServicioPropiedades servicio, ServicioVisitas servicioVisitas, ServicioAutenticacion autenticacion)
        {
            Servicio = servicio;
            ServicioVisitas = servicioVisitas;
            Autenticacion = autenticacion;
        }

        public ObservableCollection<VisitaDTO> Visitas { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PuedeEditar))]
        public partial PropiedadDTO? Propiedad { get; set; }

        [ObservableProperty] public partial bool Cargando { get; set; }

        public bool PuedeEditar => Propiedad != null &&
            (Autenticacion.EsAdmin || Propiedad.IdTrabajador == Autenticacion.UsuarioActual?.IdTrabajador);

        public event Action<PropiedadDTO>? PropiedadCargada;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Id", out var Valor) && int.TryParse(Valor?.ToString(), out var Cod))
                Id = Cod;
        }

        public async Task Cargar()
        {
            if (Id == 0) return;
            try
            {
                Cargando = true;
                Propiedad = await Servicio.Buscar(Id);
                PropiedadCargada?.Invoke(Propiedad);
                Visitas.Clear();
                foreach (var V in (await ServicioVisitas.PorPropiedad(Id)).OrderByDescending(V => V.FechaVisita))
                    Visitas.Add(V);
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

        [RelayCommand]
        private Task Editar() => Shell.Current.GoToAsync(Rutas.ConId(Rutas.PropiedadForm, Id));

        [RelayCommand]
        private Task ProgramarVisita() => Shell.Current.GoToAsync($"{Rutas.VisitaForm}?IdPropiedad={Id}");
    }
}
