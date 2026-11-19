namespace TerraNova.Services
{
    public static class Configuracion
    {
        // Puerto definido en TerraNova.Api/Properties/launchSettings.json
        private const int Puerto = 5150;

        // - Windows: la API esta en la misma PC -> localhost
        // - Emulador Android: ve a la PC como 10.0.2.2
        // - Celular FISICO por cable USB: se ejecuta "adb reverse tcp:5150 tcp:5150" y el celular
        //   usa su propio localhost, que el cable redirige a la PC (no hace falta Wi-Fi ni IP).
        //   Alternativa por Wi-Fi: reemplazar por la IP de la PC, ej: "http://192.168.1.10:5150/"
        public static string UrlApi =>
            DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.DeviceType == DeviceType.Virtual
                ? $"http://10.0.2.2:{Puerto}/"
                : $"http://localhost:{Puerto}/";
    }
}
