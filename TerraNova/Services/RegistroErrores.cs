namespace TerraNova.Services
{
    // Guarda en un archivo cualquier error no controlado (con su detalle completo) para poder diagnosticarlo.
    // Archivo: FileSystem.AppDataDirectory/errores.log
    //   Windows: %LOCALAPPDATA%\User Name\com.terranova.bienesraices\Data\errores.log
    public static class RegistroErrores
    {
        public static string Ruta => Path.Combine(FileSystem.AppDataDirectory, "errores.log");

        public static void Activar()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, E) => Guardar("AppDomain", E.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (_, E) => { Guardar("Task", E.Exception); E.SetObserved(); };
        }

        public static void Guardar(string Origen, Exception? Ex)
        {
            try
            {
                File.AppendAllText(Ruta, $"==== {DateTime.Now:yyyy-MM-dd HH:mm:ss} [{Origen}]\n{Ex}\n\n");
            }
            catch { /* si no se puede escribir el registro, no hacer nada */ }
        }
    }
}
