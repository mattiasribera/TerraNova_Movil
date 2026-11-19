using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Services;
using TerraNova.Shared;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Usuarios/RegistrarUsuario.razor
    public partial class UsuarioFormViewModel : FormularioViewModel
    {
        private readonly ServicioUsuarios Servicio;
        private readonly ServicioTrabajadores ServicioTrabajadores;

        public UsuarioFormViewModel(ServicioUsuarios servicio, ServicioTrabajadores servicioTrabajadores)
        {
            Servicio = servicio;
            ServicioTrabajadores = servicioTrabajadores;
            Titulo = "Registrar usuario";
        }

        public List<string> Roles { get; } = new() { "Agente", "Admin" };
        public ObservableCollection<TrabajadorDTO> Trabajadores { get; } = new();
        public bool EsNuevo => !EsEdicion;

        [ObservableProperty]
        public partial UsuarioDTO Usuario { get; set; } = new() { NombreUsuario = "", Rol = "Agente", Activo = true };

        [ObservableProperty] public partial string Contrasena { get; set; } = string.Empty;
        [ObservableProperty] public partial TrabajadorDTO? Trabajador { get; set; }

        protected override async Task CargarDatos()
        {
            foreach (var T in await ServicioTrabajadores.Activos())
                Trabajadores.Add(T);
            if (EsEdicion)
            {
                Titulo = "Editar usuario";
                Usuario = await Servicio.Buscar(Id!.Value);
                Trabajador = Trabajadores.FirstOrDefault(T => T.Id == Usuario.IdTrabajador);
            }
            OnPropertyChanged(nameof(EsNuevo));
        }

        [RelayCommand]
        private void QuitarTrabajador() => Trabajador = null;

        [RelayCommand]
        private async Task Guardar()
        {
            var U = Usuario;
            if (string.IsNullOrWhiteSpace(U.NombreUsuario))
            {
                await Dialogos.Alerta("Datos incompletos", "El nombre de usuario es obligatorio");
                return;
            }
            if (!EsEdicion && Contrasena.Length < 6)
            {
                await Dialogos.Alerta("Datos incompletos", "La contraseña debe tener al menos 6 caracteres");
                return;
            }
            try
            {
                if (await Servicio.ExisteNombreUsuario(U.NombreUsuario, Id ?? 0))
                {
                    await Dialogos.Alerta("Duplicado", "Ya existe un usuario con ese nombre.");
                    return;
                }
            }
            catch (Exception Ex)
            {
                await Dialogos.Alerta("Error", Ex.Message);
                return;
            }

            U.IdTrabajador = Trabajador?.Id;
            U.IdTrabajadorNavigation = null;
            // La API hashea con BCrypt el texto que llega en ContraseñaHash (solo al registrar)
            if (!EsEdicion) U.ContraseñaHash = Contrasena;
            await GuardarYVolver(() => EsEdicion ? Servicio.Modificar(U) : Servicio.Guardar(U), "Usuario");
        }
    }
}
