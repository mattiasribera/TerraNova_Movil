using System.Collections.Concurrent;
using System.Globalization;
using TerraNova.Services;

namespace TerraNova.Converters
{
    // Convierte lo que viene en FotoUrl / ImagenUrl en algo que Image pueda mostrar:
    //   "imagenes/propiedades/x.jpg"  -> archivo servido por la API (formato nuevo, liviano)
    //   "https://..."                 -> URL externa
    //   "data:image/...;base64,..."   -> formato antiguo de la web (se sigue pudiendo mostrar)
    //
    // Las imagenes se descargan con nuestro propio HttpClient (en vez de dejarselo a MAUI) y se
    // guardan en memoria: asi funcionan igual en Windows y Android y las listas no las descargan dos veces.
    public class FotoAImagenConverter : IValueConverter
    {
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };
        private static readonly ConcurrentDictionary<string, byte[]> Cache = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string Texto || string.IsNullOrWhiteSpace(Texto))
                return null;

            if (Texto.StartsWith("data:"))
            {
                var Coma = Texto.IndexOf(',');
                if (Coma < 0) return null;
                var Bytes = System.Convert.FromBase64String(Texto[(Coma + 1)..]);
                return ImageSource.FromStream(() => new MemoryStream(Bytes));
            }

            Uri? Url = Texto.StartsWith("imagenes/")
                ? new Uri(new Uri(Configuracion.UrlApi), Texto)
                : Uri.TryCreate(Texto, UriKind.Absolute, out var Externa) ? Externa : null;
            if (Url == null) return null;

            return ImageSource.FromStream(async Cancelar =>
            {
                try
                {
                    if (!Cache.TryGetValue(Url.AbsoluteUri, out var Bytes))
                    {
                        Bytes = await Http.GetByteArrayAsync(Url, Cancelar);
                        Cache[Url.AbsoluteUri] = Bytes;
                    }
                    return new MemoryStream(Bytes);
                }
                catch
                {
                    return null; // sin conexion o imagen inexistente: se ve el icono de "sin foto"
                }
            });
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
