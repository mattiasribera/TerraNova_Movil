using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using TerraNova.Converters;
using TerraNova.Shared;

namespace TerraNova.Mapas
{
    // Reemplaza a wwwroot/js/mapaInterop.js (Leaflet) de la web. Mapsui usa los mismos mapas de
    // OpenStreetMap, es gratis, no necesita API Key y funciona en Android, iOS y Windows.
    public static class MapaHelper
    {
        // Centro por defecto (Santa Cruz de la Sierra), igual que en la web
        public const double LatInicial = -17.7833;
        public const double LngInicial = -63.1821;

        public static Mapsui.Map CrearMapa(double Lat = LatInicial, double Lng = LngInicial, int Zoom = 13)
        {
            var Mapa = new Mapsui.Map();
            Mapa.Layers.Add(OpenStreetMap.CreateTileLayer("TerraNova-MAUI"));
            var Centro = APunto(Lat, Lng);
            Mapa.Navigator.CenterOnAndZoomTo(Centro, Mapa.Navigator.Resolutions[Zoom]);
            return Mapa;
        }

        // Lat/Lng (grados) -> coordenadas del mapa (metros, SphericalMercator)
        public static MPoint APunto(double Lat, double Lng)
        {
            var (X, Y) = SphericalMercator.FromLonLat(Lng, Lat);
            return new MPoint(X, Y);
        }

        // Coordenadas del mapa -> Lat/Lng
        public static (double Lat, double Lng) ALatLng(MPoint Punto)
        {
            var (Lng, Lat) = SphericalMercator.ToLonLat(Punto.X, Punto.Y);
            return (Lat, Lng);
        }

        public static PointFeature CrearMarcador(double Lat, double Lng, string? Texto = null, string? Estado = null)
        {
            var C = EstadoAColorConverter.ColorDe(Estado ?? "Disponible");
            var Marcador = new PointFeature(APunto(Lat, Lng));
            Marcador.Styles.Add(new SymbolStyle
            {
                SymbolScale = 0.7,
                Fill = new Mapsui.Styles.Brush(new Mapsui.Styles.Color((int)(C.Red * 255), (int)(C.Green * 255), (int)(C.Blue * 255))),
                Outline = new Pen(Mapsui.Styles.Color.White, 2)
            });
            if (!string.IsNullOrWhiteSpace(Texto))
            {
                Marcador.Styles.Add(new LabelStyle
                {
                    Text = Texto,
                    Offset = new Offset(0, -24),
                    BackColor = new Mapsui.Styles.Brush(Mapsui.Styles.Color.White),
                    ForeColor = Mapsui.Styles.Color.Black
                });
            }
            return Marcador;
        }

        public static MemoryLayer CrearCapaPropiedades(IEnumerable<PropiedadDTO> Propiedades)
        {
            var Marcadores = new List<IFeature>();
            foreach (var P in Propiedades.Where(P => P.Latitud.HasValue && P.Longitud.HasValue))
            {
                var M = CrearMarcador(P.Latitud!.Value, P.Longitud!.Value, P.Titulo, P.Estado);
                M["Id"] = P.Id;
                Marcadores.Add(M);
            }
            return new MemoryLayer("Propiedades") { Features = Marcadores, Style = null };
        }
    }
}
