namespace TerraNova.Models
{
    // Una tarjeta de numero grande del Inicio (ej: "Propiedades 101", "Comisión del mes $ 3.500")
    public class Indicador
    {
        public string Titulo { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;   // codigo de Constantes.Iconos
        public Color Color { get; set; } = Colors.Black;
    }
}
