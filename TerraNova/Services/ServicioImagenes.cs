using System.Net.Http.Headers;
using System.Net.Http.Json;
using SkiaSharp;
using TerraNova.Shared;

namespace TerraNova.Services
{
    // Flujo de imagenes:
    //   1. Elegir foto (galeria o camara)
    //   2. Achicarla y comprimirla en el celular (una foto de 4 MB queda en ~150 KB)
    //   3. Subirla a la API (api/Imagen/Subir) -> la API la guarda como archivo
    //   4. En la BD solo se guarda la ruta: "imagenes/trabajadores/xxxx.jpg"
    public class ServicioImagenes
    {
        private const int LadoMaximo = 1280;   // pixeles del lado mas largo
        private const int CalidadJpeg = 80;    // 0-100

        private readonly HttpClient Http;

        public ServicioImagenes(HttpClient http)
        {
            Http = http;
        }

        // Devuelve null si el usuario cancelo
        public async Task<FileResult?> Elegir(bool DesdeCamara)
        {
            if (DesdeCamara)
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                    throw new InvalidOperationException("Este dispositivo no tiene cámara.");
                return await MediaPicker.Default.CapturePhotoAsync();
            }
            var Fotos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
            return Fotos.FirstOrDefault();
        }

        public async Task<string> ComprimirYSubir(FileResult Archivo, string Carpeta)
        {
            byte[] Comprimida;
            using (var Origen = await Archivo.OpenReadAsync())
                Comprimida = Comprimir(Origen);

            using var Contenido = new MultipartFormDataContent();
            var Bytes = new ByteArrayContent(Comprimida);
            Bytes.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            Contenido.Add(Bytes, "archivo", "foto.jpg");

            var Respuesta = await Http.PostAsync($"api/Imagen/Subir/{Carpeta}", Contenido);
            var Resultado = await Respuesta.Content.ReadFromJsonAsync<ResponseAPI<string>>();
            if (Resultado != null && Resultado.EsCorrecto && !string.IsNullOrEmpty(Resultado.Valor))
                return Resultado.Valor;
            throw new Exception(Resultado?.Mensaje ?? "No se pudo subir la imagen");
        }

        // Achica la imagen (maximo 1280 px) y la guarda como JPEG calidad 80.
        // Tambien la endereza segun la orientacion EXIF (las fotos de celular suelen venir giradas).
        private static byte[] Comprimir(Stream Origen)
        {
            using var Datos = SKData.Create(Origen);
            using var Codec = SKCodec.Create(Datos) ?? throw new InvalidOperationException("El archivo no es una imagen válida.");
            using var Original = SKBitmap.Decode(Codec) ?? throw new InvalidOperationException("No se pudo leer la imagen.");
            using var Derecha = Enderezar(Original, Codec.EncodedOrigin);

            var Escala = Math.Min(1.0, (double)LadoMaximo / Math.Max(Derecha.Width, Derecha.Height));
            var Ancho = Math.Max(1, (int)(Derecha.Width * Escala));
            var Alto = Math.Max(1, (int)(Derecha.Height * Escala));

            using var Chica = Derecha.Resize(new SKImageInfo(Ancho, Alto), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear))
                              ?? throw new InvalidOperationException("No se pudo redimensionar la imagen.");
            using var Imagen = SKImage.FromBitmap(Chica);
            using var Jpeg = Imagen.Encode(SKEncodedImageFormat.Jpeg, CalidadJpeg);
            return Jpeg.ToArray();
        }

        private static SKBitmap Enderezar(SKBitmap Bmp, SKEncodedOrigin Orientacion)
        {
            int Grados = Orientacion switch
            {
                SKEncodedOrigin.RightTop => 90,
                SKEncodedOrigin.BottomRight => 180,
                SKEncodedOrigin.LeftBottom => 270,
                _ => 0
            };
            if (Grados == 0) return Bmp.Copy();

            var Girada = Grados == 180 ? new SKBitmap(Bmp.Width, Bmp.Height) : new SKBitmap(Bmp.Height, Bmp.Width);
            using var Lienzo = new SKCanvas(Girada);
            Lienzo.Translate(Girada.Width / 2f, Girada.Height / 2f);
            Lienzo.RotateDegrees(Grados);
            Lienzo.Translate(-Bmp.Width / 2f, -Bmp.Height / 2f);
            Lienzo.DrawBitmap(Bmp, 0, 0);
            return Girada;
        }
    }
}
