using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Helpers;
using TerraNova.Services;
using TerraNova.Shared;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Trabajadores/RegistrarTrabajador.razor
    public partial class TrabajadorFormViewModel : FormularioViewModel
    {
        private readonly ServicioTrabajadores Servicio;
        private readonly ServicioImagenes Imagenes;

        private readonly ServicioAutenticacion Autenticacion;

        public TrabajadorFormViewModel(ServicioTrabajadores servicio, ServicioImagenes imagenes, ServicioAutenticacion autenticacion)
        {
            Servicio = servicio;
            Imagenes = imagenes;
            Autenticacion = autenticacion;
            Titulo = "Registrar trabajador";
        }

        // Un agente edita su perfil pero no su comision, cargo ni estado (la API tambien lo protege)
        public bool EsAdmin => Autenticacion.EsAdmin;

        public List<string> Cargos { get; } = new() { "Agente", "Administrador", "Recepcionista" };

        [ObservableProperty]
        public partial TrabajadorDTO Trabajador { get; set; } = new() { Cargo = "Agente", Activo = true, FechaContratacion = DateTime.Today };

        [ObservableProperty] public partial string? Foto { get; set; }

        // Texto del campo comision: acepta decimales con coma o punto (ej: 3,5)
        [ObservableProperty] public partial string Comision { get; set; } = "0";

        protected override async Task CargarDatos()
        {
            if (!EsEdicion) return;
            Titulo = "Editar trabajador";
            Trabajador = await Servicio.Buscar(Id!.Value);
            Foto = Trabajador.FotoUrl;
            Comision = Numeros.Texto(Trabajador.PorcentajeComision);
        }

        // Elegir foto de la galeria o la camara: se comprime y se sube como archivo a la API.
        // En FotoUrl solo queda la ruta (ej: "imagenes/trabajadores/xxxx.jpg"), no el base64.
        [RelayCommand]
        private async Task ElegirFoto(string Origen)
        {
            var Ruta = await SubirFoto(Imagenes, Origen, "trabajadores");
            if (Ruta != null) Foto = Ruta;
        }

        [RelayCommand]
        private void QuitarFoto() => Foto = null;

        [RelayCommand]
        private async Task Guardar()
        {
            var T = Trabajador;
            var ComisionValida = Numeros.TryLeer(Comision, out var ValorComision) && ValorComision <= 100;
            string? Error =
                string.IsNullOrWhiteSpace(T.Nombre) ? "El nombre es obligatorio" :
                string.IsNullOrWhiteSpace(T.Apellido) ? "El apellido es obligatorio" :
                string.IsNullOrWhiteSpace(T.Ci) ? "La CI es obligatoria" :
                string.IsNullOrWhiteSpace(T.Telefono) ? "El teléfono es obligatorio" :
                string.IsNullOrWhiteSpace(T.Email) || !T.Email.Contains('@') ? "Email no válido" :
                !ComisionValida ? "La comisión debe ser un número entre 0 y 100 (ej: 3,5)" :
                null;
            if (Error != null)
            {
                await Dialogos.Alerta("Datos incompletos", Error);
                return;
            }
            T.PorcentajeComision = decimal.Round(ValorComision, 2);
            T.FotoUrl = Foto;
            await GuardarYVolver(() => EsEdicion ? Servicio.Modificar(T) : Servicio.Guardar(T), "Trabajador");
        }
    }
}
