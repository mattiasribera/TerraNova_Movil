using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace TerraNova.Services
{
    public static class Dialogos
    {
        public static Task Alerta(string Titulo, string Mensaje) =>
            Shell.Current.DisplayAlertAsync(Titulo, Mensaje, "OK");

        public static Task<bool> Confirmar(string Titulo, string Mensaje) =>
            Shell.Current.DisplayAlertAsync(Titulo, Mensaje, "Sí", "No");

        // INVESTIGACION: Toast de CommunityToolkit.Maui (reemplaza al ToastService hecho a mano en la web)
        public static Task Aviso(string Mensaje) =>
            Toast.Make(Mensaje, ToastDuration.Short, 15).Show();
    }
}
