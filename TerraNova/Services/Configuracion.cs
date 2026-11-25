namespace TerraNova.Services
{
    public static class Configuracion
    {
        // Puerto definido en TerraNova.Api/Properties/launchSettings.json
        private const int Puerto = 5150;

        // Servidor publico (HTTPS) que usa la version distribuida (Release) por defecto.
        // La API corre en la PC del equipo y se publica con un tunel de Cloudflare.
        // Si la URL del tunel cambia, el usuario la corrige en el campo "Servidor" del login sin reinstalar.
        public const string UrlPublicaPorDefecto = "https://moore-electron-latin-availability.trycloudflare.com/";

        // Clave en Preferences donde se guarda el servidor elegido por el usuario
        private const string ClaveServidor = "terranova_servidor";

        // Direccion fija del HttpClient. RedireccionServidor la reemplaza en cada peticion por UrlApi,
        // asi el servidor se puede cambiar en tiempo de ejecucion (HttpClient no permite cambiar BaseAddress).
        public static readonly Uri DireccionInterna = new("http://terranova-api/");

        // Servidor por defecto segun como se compilo la app:
        // - Debug (desarrollo):
        //   - Windows: la API esta en la misma PC -> localhost
        //   - Emulador Android: ve a la PC como 10.0.2.2
        //   - Celular FISICO por cable USB: se ejecuta "adb reverse tcp:5150 tcp:5150" y el celular
        //     usa su propio localhost, que el cable redirige a la PC.
        // - Release (APK/instalador distribuido): servidor publico HTTPS
        public static string UrlPorDefecto
        {
            get
            {
#if DEBUG
                return DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.DeviceType == DeviceType.Virtual
                    ? $"http://10.0.2.2:{Puerto}/"
                    : $"http://localhost:{Puerto}/";
#else
                return UrlPublicaPorDefecto;
#endif
            }
        }

        // Servidor actual: el que guardo el usuario o, si no hay ninguno, el de por defecto
        public static string UrlApi
        {
            get
            {
                var Guardada = Preferences.Default.Get(ClaveServidor, string.Empty);
                return string.IsNullOrWhiteSpace(Guardada) ? UrlPorDefecto : Guardada;
            }
        }

        // Valida y normaliza una URL escrita por el usuario. Devuelve null si no es valida.
        // Acepta "midominio.com" (agrega https://) y siempre termina en "/".
        public static string? NormalizarUrl(string? Texto)
        {
            if (string.IsNullOrWhiteSpace(Texto))
                return null;

            var Url = Texto.Trim();
            if (!Url.Contains("://"))
                Url = "https://" + Url;

            if (!Uri.TryCreate(Url, UriKind.Absolute, out var Direccion)
                || (Direccion.Scheme != Uri.UriSchemeHttp && Direccion.Scheme != Uri.UriSchemeHttps)
                || string.IsNullOrEmpty(Direccion.Host))
                return null;

            var Normalizada = Direccion.GetLeftPart(UriPartial.Path);
            return Normalizada.EndsWith('/') ? Normalizada : Normalizada + "/";
        }

        // Guarda el servidor elegido. Si es igual al de por defecto se borra, asi una nueva
        // version de la app con otro servidor por defecto lo toma automaticamente.
        public static void GuardarUrlApi(string UrlNormalizada)
        {
            if (UrlNormalizada == UrlPorDefecto)
                Preferences.Default.Remove(ClaveServidor);
            else
                Preferences.Default.Set(ClaveServidor, UrlNormalizada);
        }
    }

    // Reemplaza la direccion interna del HttpClient por el servidor configurado en cada peticion
    public class RedireccionServidor : DelegatingHandler
    {
        public RedireccionServidor() : base(new HttpClientHandler()) { }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage Peticion, CancellationToken Cancelar)
        {
            if (Peticion.RequestUri is { } Original && Original.Host == Configuracion.DireccionInterna.Host)
            {
                var Relativa = Original.PathAndQuery.TrimStart('/');
                Peticion.RequestUri = new Uri(new Uri(Configuracion.UrlApi), Relativa);
            }
            return base.SendAsync(Peticion, Cancelar);
        }
    }
}
