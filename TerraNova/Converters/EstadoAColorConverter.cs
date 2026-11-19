using System.Globalization;

namespace TerraNova.Converters
{
    // Mismos colores que los "badge" de estado de la version web
    public class EstadoAColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            ColorDe(value as string);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        public static Color ColorDe(string? Estado) => Estado switch
        {
            "Disponible" => Color.FromArgb("#198754"),
            "Vendida" => Color.FromArgb("#DC3545"),
            "Reservada" => Color.FromArgb("#FFC107"),
            "Alquilada" => Color.FromArgb("#0D6EFD"),
            // Visitas y ventas
            "Programada" => Color.FromArgb("#0D6EFD"),
            "Realizada" or "Completada" => Color.FromArgb("#198754"),
            "Pendiente" => Color.FromArgb("#FFC107"),
            "Cancelada" => Color.FromArgb("#DC3545"),
            // Roles de usuario
            "Admin" => Color.FromArgb("#DC3545"),
            "Agente" => Color.FromArgb("#0D6EFD"),
            _ => Colors.Gray
        };
    }
}
