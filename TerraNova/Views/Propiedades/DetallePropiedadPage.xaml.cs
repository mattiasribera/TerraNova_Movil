using Mapsui.Layers;
using TerraNova.Mapas;
using TerraNova.Shared;
using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class DetallePropiedadPage : PaginaBase
    {
        public DetallePropiedadPage(DetallePropiedadViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
            vm.PropiedadCargada += MostrarEnMapa;
        }

        private void MostrarEnMapa(PropiedadDTO P)
        {
            var TieneUbicacion = P.Latitud.HasValue && P.Longitud.HasValue;
            MarcoMapa.IsVisible = TieneUbicacion;
            SinUbicacion.IsVisible = !TieneUbicacion;
            if (!TieneUbicacion) return;

            var M = MapaHelper.CrearMapa(P.Latitud!.Value, P.Longitud!.Value, 16);
            M.Layers.Add(new MemoryLayer("Marcador")
            {
                Features = new[] { MapaHelper.CrearMarcador(P.Latitud.Value, P.Longitud.Value, P.Titulo, P.Estado) },
                Style = null
            });
            Mapa.Map = M;
        }
    }
}
