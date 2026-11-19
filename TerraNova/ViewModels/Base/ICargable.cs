namespace TerraNova.ViewModels
{
    // Lo implementan los ViewModels que deben cargar datos cada vez que se muestra su pagina
    public interface ICargable
    {
        Task Cargar();
    }
}
