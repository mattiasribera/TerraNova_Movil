using System.Globalization;

namespace TerraNova.Helpers
{
    // En Bolivia (es-BO) el separador decimal es la coma, pero muchos escriben con punto.
    // Estos metodos aceptan los dos ("3,5" y "3.5") para que ningun campo decimal falle.
    public static class Numeros
    {
        // Patron para TextValidationBehavior: entero o decimal con hasta 2 decimales, con coma o punto
        public const string PatronDecimal = @"^\d+([.,]\d{1,2})?$";

        public static bool TryLeer(string? Texto, out decimal Valor)
        {
            Valor = 0;
            if (string.IsNullOrWhiteSpace(Texto)) return false;
            var Normalizado = Texto.Trim().Replace(',', '.');
            if (Normalizado.Count(C => C == '.') > 1) return false;
            return decimal.TryParse(Normalizado, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out Valor);
        }

        // Para mostrar en un Entry: sin ceros de mas y con el separador del sistema (ej: "3,5")
        public static string Texto(decimal Valor) => Valor.ToString("0.##", CultureInfo.CurrentCulture);

        public static string Texto(double Valor) => Texto((decimal)Valor);
    }
}
