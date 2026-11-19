namespace TerraNova.Api.Services
{
    // Guarda las imagenes como ARCHIVOS en wwwroot/imagenes/{carpeta}/ y en la BD solo queda la ruta corta
    // (ej: "imagenes/trabajadores/3f2a...c9.jpg"). Asi la base de datos no se llena de base64.
    public class ServicioImagenes
    {
        public const long TamanoMaximo = 5 * 1024 * 1024; // 5 MB por archivo
        public static readonly string[] CarpetasPermitidas = { "trabajadores", "propiedades" };
        private static readonly Dictionary<string, string> Extensiones = new()
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

        private readonly string RaizWeb;

        public ServicioImagenes(IWebHostEnvironment entorno)
        {
            RaizWeb = entorno.WebRootPath ?? Path.Combine(entorno.ContentRootPath, "wwwroot");
        }

        public async Task<string> Guardar(IFormFile Archivo, string Carpeta)
        {
            if (!CarpetasPermitidas.Contains(Carpeta))
                throw new InvalidOperationException("Carpeta no permitida");
            if (Archivo.Length == 0 || Archivo.Length > TamanoMaximo)
                throw new InvalidOperationException("La imagen debe pesar entre 1 byte y 5 MB");

            // No se confia en el nombre ni en el Content-Type: se revisa la "firma" real del archivo
            using var Memoria = new MemoryStream();
            await Archivo.CopyToAsync(Memoria);
            var Bytes = Memoria.ToArray();
            var Tipo = DetectarTipo(Bytes) ?? throw new InvalidOperationException("Solo se aceptan imágenes JPG, PNG o WEBP");

            var Directorio = Path.Combine(RaizWeb, "imagenes", Carpeta);
            Directory.CreateDirectory(Directorio);
            var Nombre = $"{Guid.NewGuid():N}{Extensiones[Tipo]}";
            await File.WriteAllBytesAsync(Path.Combine(Directorio, Nombre), Bytes);

            return $"imagenes/{Carpeta}/{Nombre}";
        }

        // Borra el archivo si la ruta es una imagen nuestra (las URLs externas https://... no se tocan)
        public void Eliminar(string? Ruta)
        {
            if (string.IsNullOrWhiteSpace(Ruta) || !Ruta.StartsWith("imagenes/")) return;
            var Completa = Path.GetFullPath(Path.Combine(RaizWeb, Ruta));
            var Base = Path.GetFullPath(Path.Combine(RaizWeb, "imagenes"));
            if (Completa.StartsWith(Base) && File.Exists(Completa))
                File.Delete(Completa);
        }

        // Si se cambio la imagen, borra la anterior para no dejar archivos huerfanos
        public void ReemplazarSiCambio(string? Anterior, string? Nueva)
        {
            if (Anterior != Nueva) Eliminar(Anterior);
        }

        public static bool EsBase64(string? Valor) => Valor != null && Valor.StartsWith("data:");

        private static string? DetectarTipo(byte[] B)
        {
            if (B.Length >= 3 && B[0] == 0xFF && B[1] == 0xD8 && B[2] == 0xFF) return "image/jpeg";
            if (B.Length >= 8 && B[0] == 0x89 && B[1] == 0x50 && B[2] == 0x4E && B[3] == 0x47) return "image/png";
            if (B.Length >= 12 && B[0] == 'R' && B[1] == 'I' && B[2] == 'F' && B[3] == 'F' &&
                B[8] == 'W' && B[9] == 'E' && B[10] == 'B' && B[11] == 'P') return "image/webp";
            return null;
        }
    }
}
