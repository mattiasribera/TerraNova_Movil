namespace TerraNova.Services
{
    public static class Configuracion
    {
        // Puerto definido en TerraNova.Api/Properties/launchSettings.json
        private const int Puerto = 5150;

        // El emulador de Android ve a la PC como 10.0.2.2; Windows usa localhost.
        // Para un celular FISICO, cambie esto por la IP de su PC en la misma red Wi-Fi (ej: "http://192.168.1.10:5150/").
        public static string UrlApi =>
            DeviceInfo.Platform == DevicePlatform.Android
                ? $"http://10.0.2.2:{Puerto}/"
                : $"http://localhost:{Puerto}/";
    }
}
