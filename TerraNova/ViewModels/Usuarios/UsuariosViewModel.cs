using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Models;
using TerraNova.Services;
using TerraNova.Shared;
using TerraNova.Constantes;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Usuarios/ListarUsuarios.razor (solo Admin)
    public partial class UsuariosViewModel : ObservableObject, ICargable
    {
        private readonly ServicioUsuarios Servicio;
        private readonly ServicioTrabajadores ServicioTrabajadores;
        private readonly ServicioAutenticacion Autenticacion;

        public UsuariosViewModel(ServicioUsuarios servicio, ServicioTrabajadores servicioTrabajadores, ServicioAutenticacion autenticacion)
        {
            Servicio = servicio;
            ServicioTrabajadores = servicioTrabajadores;
            Autenticacion = autenticacion;
        }

        public ObservableCollection<UsuarioFila> Usuarios { get; } = new();

        [ObservableProperty] public partial bool Cargando { get; set; }

        [RelayCommand]
        public async Task Cargar()
        {
            if (!Autenticacion.EsAdmin)
            {
                await Dialogos.Alerta("Sin permiso", "Solo un administrador puede gestionar usuarios.");
                await Shell.Current.GoToAsync(Rutas.Inicio);
                return;
            }
            try
            {
                Cargando = true;
                var TareaUsuarios = Servicio.Lista();
                var TareaTrabajadores = ServicioTrabajadores.Lista();
                await Task.WhenAll(TareaUsuarios, TareaTrabajadores);
                var Nombres = TareaTrabajadores.Result.ToDictionary(T => T.Id, T => $"{T.Nombre} {T.Apellido}");

                Usuarios.Clear();
                foreach (var U in TareaUsuarios.Result)
                    Usuarios.Add(new UsuarioFila
                    {
                        Usuario = U,
                        Trabajador = U.IdTrabajador.HasValue && Nombres.TryGetValue(U.IdTrabajador.Value, out var N) ? N : "—"
                    });
            }
            catch (Exception Ex) { await Dialogos.Alerta("Error", Ex.Message); }
            finally { Cargando = false; }
        }

        [RelayCommand] private Task Nuevo() => Shell.Current.GoToAsync(Rutas.UsuarioForm);
        [RelayCommand] private Task Editar(UsuarioFila F) => Shell.Current.GoToAsync(Rutas.ConId(Rutas.UsuarioForm, F.Usuario.Id));

        [RelayCommand]
        private async Task Eliminar(UsuarioFila F)
        {
            if (F.Usuario.Id == Autenticacion.UsuarioActual?.Id)
            {
                await Dialogos.Alerta("No permitido", "No puede eliminar su propio usuario.");
                return;
            }
            if (!await Dialogos.Confirmar("Eliminar", $"¿Desactivar al usuario '{F.Usuario.NombreUsuario}'?")) return;
            try
            {
                await Servicio.Eliminar(F.Usuario.Id);
                await Dialogos.Aviso("Usuario desactivado");
                await Cargar();
            }
            catch (Exception Ex) { await Dialogos.Alerta("No se pudo eliminar", Ex.Message); }
        }
    }
}
