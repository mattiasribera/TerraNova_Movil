using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Services;
using TerraNova.Shared;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Clientes/RegistrarCliente.razor
    public partial class ClienteFormViewModel : FormularioViewModel
    {
        private readonly ServicioClientes Servicio;

        public ClienteFormViewModel(ServicioClientes servicio)
        {
            Servicio = servicio;
            Titulo = "Registrar cliente";
        }

        public List<string> TiposCliente { get; } = new() { "Comprador", "Vendedor", "Ambos" };

        [ObservableProperty]
        public partial ClienteDTO Cliente { get; set; } = new() { TipoCliente = "Comprador", FechaRegistro = DateTime.Now };

        protected override async Task CargarDatos()
        {
            if (!EsEdicion) return;
            Titulo = "Editar cliente";
            Cliente = await Servicio.Buscar(Id!.Value);
        }

        [RelayCommand]
        private async Task Guardar()
        {
            var C = Cliente;
            string? Error =
                string.IsNullOrWhiteSpace(C.Nombre) ? "El nombre es obligatorio" :
                string.IsNullOrWhiteSpace(C.Apellido) ? "El apellido es obligatorio" :
                string.IsNullOrWhiteSpace(C.Ci) ? "La CI es obligatoria" :
                string.IsNullOrWhiteSpace(C.Telefono) ? "El teléfono es obligatorio" :
                string.IsNullOrWhiteSpace(C.Email) || !C.Email.Contains('@') ? "Email no válido" :
                string.IsNullOrWhiteSpace(C.Direccion) ? "La dirección es obligatoria" :
                null;
            if (Error != null)
            {
                await Dialogos.Alerta("Datos incompletos", Error);
                return;
            }

            // Mismas validaciones de duplicados que la web (consultan a la API)
            try
            {
                var IdExcluir = Id ?? 0;
                var Errores = new List<string>();
                if (await Servicio.ExisteCi(C.Ci, IdExcluir)) Errores.Add("Ya existe un cliente con esa CI.");
                if (await Servicio.ExisteTelefono(C.Telefono, IdExcluir)) Errores.Add("Ya existe un cliente con ese teléfono.");
                if (await Servicio.ExisteEmail(C.Email, IdExcluir)) Errores.Add("Ya existe un cliente con ese email.");
                if (Errores.Count > 0)
                {
                    await Dialogos.Alerta("Datos duplicados", string.Join("\n", Errores));
                    return;
                }
            }
            catch (Exception Ex)
            {
                await Dialogos.Alerta("Error", Ex.Message);
                return;
            }

            if (!EsEdicion) C.FechaRegistro = DateTime.Now;
            await GuardarYVolver(() => EsEdicion ? Servicio.Modificar(C) : Servicio.Guardar(C), "Cliente");
        }
    }
}
