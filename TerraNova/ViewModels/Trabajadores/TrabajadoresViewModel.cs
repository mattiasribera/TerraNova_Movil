using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Services;
using TerraNova.Shared;
using TerraNova.Constantes;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Trabajadores/ListarTrabajadores.razor
    public partial class TrabajadoresViewModel : ObservableObject, ICargable
    {
        private readonly ServicioTrabajadores Servicio;
        private readonly ServicioAutenticacion Autenticacion;
        private List<TrabajadorDTO> Todos = new();

        public TrabajadoresViewModel(ServicioTrabajadores servicio, ServicioAutenticacion autenticacion)
        {
            Servicio = servicio;
            Autenticacion = autenticacion;
        }

        public bool EsAdmin => Autenticacion.EsAdmin;

        public ObservableCollection<TrabajadorDTO> Trabajadores { get; } = new();

        [ObservableProperty] public partial string TextoBusqueda { get; set; } = string.Empty;
        [ObservableProperty] public partial bool Cargando { get; set; }

        partial void OnTextoBusquedaChanged(string value) => Filtrar();

        [RelayCommand]
        public async Task Cargar()
        {
            try
            {
                Cargando = true;
                Todos = await Servicio.Lista();
                Filtrar();
            }
            catch (Exception Ex) { await Dialogos.Alerta("Error", Ex.Message); }
            finally { Cargando = false; }
        }

        private void Filtrar()
        {
            var Texto = TextoBusqueda.Trim().ToLower();
            Trabajadores.Clear();
            foreach (var T in Todos.Where(T => Texto == "" || T.Nombre.ToLower().Contains(Texto) ||
                                               T.Apellido.ToLower().Contains(Texto) || T.Cargo.ToLower().Contains(Texto)))
                Trabajadores.Add(T);
        }

        [RelayCommand]
        private async Task Nuevo()
        {
            if (!Autenticacion.EsAdmin) { await Dialogos.Alerta("Sin permiso", "Solo un administrador puede registrar trabajadores."); return; }
            await Shell.Current.GoToAsync(Rutas.TrabajadorForm);
        }

        [RelayCommand]
        private async Task Editar(TrabajadorDTO T)
        {
            if (!Autenticacion.EsAdmin && T.Id != Autenticacion.UsuarioActual?.IdTrabajador)
            {
                await Dialogos.Alerta("Sin permiso", "Solo puede editar sus propios datos.");
                return;
            }
            await Shell.Current.GoToAsync(Rutas.ConId(Rutas.TrabajadorForm, T.Id));
        }

        [RelayCommand]
        private async Task DarDeBaja(TrabajadorDTO T)
        {
            if (!Autenticacion.EsAdmin) { await Dialogos.Alerta("Sin permiso", "Solo un administrador puede dar de baja."); return; }
            if (!await Dialogos.Confirmar("Dar de baja", $"¿Dar de baja a {T.Nombre} {T.Apellido}?")) return;
            try
            {
                await Servicio.Eliminar(T.Id);
                await Dialogos.Aviso("Trabajador dado de baja");
                await Cargar();
            }
            catch (Exception Ex) { await Dialogos.Alerta("No se pudo dar de baja", Ex.Message); }
        }
    }
}
