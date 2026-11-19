using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TerraNova.Helpers;
using TerraNova.Services;
using TerraNova.Shared;
using TerraNova.Constantes;

namespace TerraNova.ViewModels
{
    // Equivalente a Pages/Propiedades/RegistrarPropiedad.razor (registrar y editar)
    public partial class PropiedadFormViewModel : FormularioViewModel
    {
        private readonly ServicioPropiedades Servicio;
        private readonly ServicioTrabajadores ServicioTrabajadores;
        private readonly ServicioAutenticacion Autenticacion;
        private readonly ServicioImagenes Imagenes;

        public PropiedadFormViewModel(ServicioPropiedades servicio, ServicioTrabajadores servicioTrabajadores,
                                      ServicioAutenticacion autenticacion, ServicioImagenes imagenes)
        {
            Servicio = servicio;
            ServicioTrabajadores = servicioTrabajadores;
            Autenticacion = autenticacion;
            Imagenes = imagenes;
            Titulo = "Registrar propiedad";
        }

        // Ruta de la imagen ("imagenes/propiedades/xxx.jpg") o URL externa
        [ObservableProperty] public partial string? Imagen { get; set; }

        [RelayCommand]
        private async Task ElegirFoto(string Origen)
        {
            var Ruta = await SubirFoto(Imagenes, Origen, "propiedades");
            if (Ruta != null) Imagen = Ruta;
        }

        [RelayCommand]
        private void QuitarFoto() => Imagen = null;

        public List<string> Tipos { get; } = new() { "Casa", "Departamento", "Terreno", "Oficina", "Local" };
        public List<string> Operaciones { get; } = new() { "Venta", "Alquiler" };
        public List<string> Estados { get; } = new() { "Disponible", "Reservada", "Vendida", "Alquilada" };
        public ObservableCollection<TrabajadorDTO> Trabajadores { get; } = new();
        public bool EsAdmin => Autenticacion.EsAdmin;

        [ObservableProperty]
        public partial PropiedadDTO Propiedad { get; set; } = new() { Estado = "Disponible", TipoOperacion = "Venta", FechaPublicacion = DateTime.Now };

        [ObservableProperty] public partial TrabajadorDTO? Trabajador { get; set; }

        // Texto de los campos numericos: empiezan vacios (no "0") y se validan al guardar
        [ObservableProperty] public partial string Precio { get; set; } = string.Empty;
        [ObservableProperty] public partial string Area { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextoUbicacion))]
        public partial double? Latitud { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextoUbicacion))]
        public partial double? Longitud { get; set; }

        public string TextoUbicacion => Latitud.HasValue
            ? $"Lat: {Latitud:F5}   Lng: {Longitud:F5}"
            : "Toque el mapa para marcar la ubicación";

        // La pagina escucha este evento para centrar el mapa al editar
        public event Action<double, double>? UbicacionCargada;

        protected override async Task CargarDatos()
        {
            foreach (var T in await ServicioTrabajadores.Activos())
                Trabajadores.Add(T);

            if (EsEdicion)
            {
                Titulo = "Editar propiedad";
                Propiedad = await Servicio.Buscar(Id!.Value);
                if (!EsAdmin && Propiedad.IdTrabajador != Autenticacion.UsuarioActual?.IdTrabajador)
                {
                    await Dialogos.Alerta("Sin permiso", "Solo puede editar las propiedades que tiene asignadas.");
                    await Shell.Current.GoToAsync(Rutas.Atras);
                    return;
                }
                Imagen = Propiedad.ImagenUrl;
                Precio = Numeros.Texto(Propiedad.Precio);
                Area = Numeros.Texto(Propiedad.AreaM2);
                Latitud = Propiedad.Latitud;
                Longitud = Propiedad.Longitud;
                if (Latitud.HasValue && Longitud.HasValue)
                    UbicacionCargada?.Invoke(Latitud.Value, Longitud.Value);
            }
            else if (!EsAdmin)
            {
                // Un agente registra propiedades a su nombre
                Propiedad.IdTrabajador = Autenticacion.UsuarioActual?.IdTrabajador ?? 0;
            }
            Trabajador = Trabajadores.FirstOrDefault(T => T.Id == Propiedad.IdTrabajador);
        }

        [RelayCommand]
        private async Task Guardar()
        {
            var P = Propiedad;
            var PrecioValido = Numeros.TryLeer(Precio, out var ValorPrecio) && ValorPrecio > 0;
            var AreaValida = Numeros.TryLeer(Area, out var ValorArea) && ValorArea > 0;
            string? Error =
                string.IsNullOrWhiteSpace(P.Titulo) ? "El título es obligatorio" :
                string.IsNullOrWhiteSpace(P.Tipo) ? "El tipo es obligatorio" :
                string.IsNullOrWhiteSpace(P.Direccion) ? "La dirección es obligatoria" :
                string.IsNullOrWhiteSpace(P.Zona) ? "La zona es obligatoria" :
                !PrecioValido ? "El precio debe ser un número mayor a 0" :
                !AreaValida ? "El área debe ser un número mayor a 0" :
                Trabajador == null ? "Debe elegir el agente asignado" :
                null;
            if (Error != null)
            {
                await Dialogos.Alerta("Datos incompletos", Error);
                return;
            }

            P.Precio = ValorPrecio;
            P.AreaM2 = (double)ValorArea;
            P.IdTrabajador = Trabajador!.Id;
            P.ImagenUrl = Imagen ?? string.Empty;
            P.Latitud = Latitud;
            P.Longitud = Longitud;
            P.IdTrabajadorNavigation = null;
            await GuardarYVolver(() => EsEdicion ? Servicio.Modificar(P) : Servicio.Guardar(P), "Propiedad");
        }
    }
}
