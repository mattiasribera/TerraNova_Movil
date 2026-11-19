using CommunityToolkit.Mvvm.ComponentModel;
using TerraNova.Services;
using TerraNova.Constantes;

namespace TerraNova.ViewModels
{
    // Base para los formularios de Registrar/Editar: recibe ?Id= por la navegacion de Shell
    public abstract partial class FormularioViewModel : ObservableObject, IQueryAttributable, ICargable
    {
        private bool Cargado;
        protected int? Id;

        public bool EsEdicion => Id.HasValue;

        [ObservableProperty]
        public partial string Titulo { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool Ocupado { get; set; }

        public virtual void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Id", out var Valor) && int.TryParse(Valor?.ToString(), out var Cod))
                Id = Cod;
        }

        public async Task Cargar()
        {
            if (Cargado) return;
            Cargado = true;
            try
            {
                Ocupado = true;
                await CargarDatos();
            }
            catch (Exception Ex)
            {
                await Dialogos.Alerta("Error", "No se pudieron cargar los datos: " + Ex.Message);
            }
            finally
            {
                Ocupado = false;
            }
        }

        protected abstract Task CargarDatos();

        // Boton "Cancelar" de todos los formularios
        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private Task Cancelar() => Shell.Current.GoToAsync(Rutas.Atras);

        [ObservableProperty]
        public partial bool SubiendoFoto { get; set; }

        // Elige una foto (Origen = "Camara" o "Galeria"), la comprime y la sube a la API.
        // Devuelve la ruta corta para guardar en la BD, o null si se cancelo o fallo.
        protected async Task<string?> SubirFoto(Services.ServicioImagenes Imagenes, string Origen, string Carpeta)
        {
            try
            {
                var Archivo = await Imagenes.Elegir(Origen == "Camara");
                if (Archivo == null) return null;
                SubiendoFoto = true;
                var Ruta = await Imagenes.ComprimirYSubir(Archivo, Carpeta);
                await Dialogos.Aviso("Foto subida");
                return Ruta;
            }
            catch (Exception Ex)
            {
                await Dialogos.Alerta("Foto", "No se pudo cargar la imagen: " + Ex.Message);
                return null;
            }
            finally
            {
                SubiendoFoto = false;
            }
        }

        // Ejecuta el guardado con manejo de errores y vuelve a la lista
        protected async Task GuardarYVolver(Func<Task> Accion, string Entidad)
        {
            if (Ocupado) return;
            try
            {
                Ocupado = true;
                await Accion();
                await Dialogos.Aviso($"{Entidad} {(EsEdicion ? "modificado(a)" : "registrado(a)")} correctamente");
                await Shell.Current.GoToAsync(Rutas.Atras);
            }
            catch (Exception Ex)
            {
                await Dialogos.Alerta("No se pudo guardar", Ex.Message);
            }
            finally
            {
                Ocupado = false;
            }
        }
    }
}
