using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Services;
using TerraNova.Shared;
using TerraNova.Constantes;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Clientes/ListarClientes.razor
    public partial class ClientesViewModel : ObservableObject, ICargable
    {
        private readonly ServicioClientes Servicio;
        private List<ClienteDTO> Todos = new();

        private readonly ServicioAutenticacion Autenticacion;

        public ClientesViewModel(ServicioClientes servicio, ServicioAutenticacion autenticacion)
        {
            Servicio = servicio;
            Autenticacion = autenticacion;
        }

        // Solo el Admin puede eliminar clientes (la API tambien lo valida)
        public bool EsAdmin => Autenticacion.EsAdmin;

        public ObservableCollection<ClienteDTO> Clientes { get; } = new();

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
            Clientes.Clear();
            foreach (var C in Todos.Where(C => Texto == "" || C.Nombre.ToLower().Contains(Texto) ||
                                               C.Apellido.ToLower().Contains(Texto) || C.Ci.ToLower().Contains(Texto)))
                Clientes.Add(C);
        }

        [RelayCommand] private Task Nuevo() => Shell.Current.GoToAsync(Rutas.ClienteForm);
        [RelayCommand] private Task Editar(ClienteDTO C) => Shell.Current.GoToAsync(Rutas.ConId(Rutas.ClienteForm, C.Id));

        [RelayCommand]
        private async Task Llamar(ClienteDTO C)
        {
            if (PhoneDialer.Default.IsSupported) PhoneDialer.Default.Open(C.Telefono);
            else await Dialogos.Alerta("Teléfono", C.Telefono);
        }

        [RelayCommand]
        private async Task Eliminar(ClienteDTO C)
        {
            if (!await Dialogos.Confirmar("Eliminar", $"¿Eliminar al cliente {C.Nombre} {C.Apellido}?")) return;
            try
            {
                await Servicio.Eliminar(C.Id);
                Todos.Remove(C);
                Clientes.Remove(C);
                await Dialogos.Aviso("Cliente eliminado");
            }
            catch (Exception Ex) { await Dialogos.Alerta("No se pudo eliminar", Ex.Message); }
        }
    }
}
