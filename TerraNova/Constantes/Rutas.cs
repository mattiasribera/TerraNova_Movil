namespace TerraNova.Constantes
{
    // Todas las rutas de navegacion en un solo lugar (evita escribir textos sueltos en cada ViewModel)
    public static class Rutas
    {
        // Rutas absolutas (secciones del menu)
        public const string Login = "//Login";
        public const string Inicio = "//Inicio";

        // Volver a la pagina anterior
        public const string Atras = "..";

        // Paginas de detalle y formularios (registradas en AppShell)
        public const string DetallePropiedad = "DetallePropiedad";
        public const string PropiedadForm = "PropiedadForm";
        public const string ClienteForm = "ClienteForm";
        public const string TrabajadorForm = "TrabajadorForm";
        public const string UsuarioForm = "UsuarioForm";
        public const string VentaForm = "VentaForm";
        public const string VisitaForm = "VisitaForm";

        // Ej: Rutas.ConId(Rutas.ClienteForm, 5) -> "ClienteForm?Id=5"
        public static string ConId(string Ruta, int Id) => $"{Ruta}?Id={Id}";
    }
}
