using Mapsui.Layers;
using TerraNova.Mapas;
using TerraNova.Services;
using TerraNova.Constantes;

namespace TerraNova.Views
{
    public partial class MapaPropiedadesPage : ContentPage
    {
        private readonly ServicioPropiedades Servicio;
        private MemoryLayer? CapaPropiedades;

        public MapaPropiedadesPage(ServicioPropiedades servicio)
        {
            InitializeComponent();
            Servicio = servicio;
            Mapa.Map = MapaHelper.CrearMapa();
            Mapa.Map.Tapped += AlTocarMapa;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                var Lista = await Servicio.Lista();
                if (CapaPropiedades != null)
                    Mapa.Map.Layers.Remove(CapaPropiedades);
                CapaPropiedades = MapaHelper.CrearCapaPropiedades(Lista);
                Mapa.Map.Layers.Add(CapaPropiedades);
            }
            catch (Exception Ex)
            {
                await DisplayAlertAsync("Error", Ex.Message, "OK");
            }
        }

        private void AlTocarMapa(object? sender, Mapsui.MapEventArgs e)
        {
            if (CapaPropiedades == null) return;
            var Info = e.GetMapInfo(new[] { CapaPropiedades });
            if (Info?.Feature?["Id"] is int Id)
            {
                e.Handled = true;
                MainThread.BeginInvokeOnMainThread(async () =>
                    await Shell.Current.GoToAsync(Rutas.ConId(Rutas.DetallePropiedad, Id)));
            }
        }
    }
}
