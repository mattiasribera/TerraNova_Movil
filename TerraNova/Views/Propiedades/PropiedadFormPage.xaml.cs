using Mapsui.Layers;
using TerraNova.Mapas;
using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class PropiedadFormPage : PaginaBase
    {
        private readonly PropiedadFormViewModel Vm;
        private readonly MemoryLayer CapaMarcador = new("Marcador") { Style = null };

        public PropiedadFormPage(PropiedadFormViewModel vm)
        {
            InitializeComponent();
            BindingContext = Vm = vm;

            // Igual que en la web: al tocar el mapa se coloca/mueve el marcador y se guardan Latitud/Longitud
            Mapa.Map = MapaHelper.CrearMapa();
            Mapa.Map.Layers.Add(CapaMarcador);
            Mapa.Map.Tapped += AlTocarMapa;

            // Al editar: centrar el mapa en la ubicacion guardada
            vm.UbicacionCargada += (Lat, Lng) =>
            {
                PonerMarcador(Lat, Lng);
                Mapa.Map.Navigator.CenterOnAndZoomTo(MapaHelper.APunto(Lat, Lng), Mapa.Map.Navigator.Resolutions[16]);
            };
        }

        private void AlTocarMapa(object? sender, Mapsui.MapEventArgs e)
        {
            var (Lat, Lng) = MapaHelper.ALatLng(e.WorldPosition);
            Vm.Latitud = Lat;
            Vm.Longitud = Lng;
            PonerMarcador(Lat, Lng);
            e.Handled = true;
        }

        private void PonerMarcador(double Lat, double Lng)
        {
            CapaMarcador.Features = new[] { MapaHelper.CrearMarcador(Lat, Lng) };
            CapaMarcador.DataHasChanged();
        }
    }
}
