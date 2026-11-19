using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace TerraNova.Api.Services
{
    // Crea la base de datos automaticamente la PRIMERA vez que se ejecuta la API en una PC nueva.
    // Ejecuta BaseDatos/ScriptCompleto.sql (tablas + datos de prueba + usuario admin).
    // Si la base ya existe, no hace nada: nunca borra ni modifica datos existentes.
    public static class InicializadorBaseDatos
    {
        // Devuelve true si la base se creo en esta ejecucion
        public static bool Ejecutar(string? CadenaConexion, ILogger Log)
        {
            if (string.IsNullOrWhiteSpace(CadenaConexion))
                throw new InvalidOperationException("Falta la cadena de conexión 'CadenaSql' (ver appsettings.Local.example.json).");

            var Datos = new SqlConnectionStringBuilder(CadenaConexion);
            var NombreBd = string.IsNullOrWhiteSpace(Datos.InitialCatalog) ? "TerraNovaDB" : Datos.InitialCatalog;

            // Para crear la base hay que conectarse a "master"
            Datos.InitialCatalog = "master";
            using var Conexion = new SqlConnection(Datos.ConnectionString);
            Conexion.Open();

            using (var Existe = new SqlCommand("SELECT DB_ID(@Nombre)", Conexion))
            {
                Existe.Parameters.AddWithValue("@Nombre", NombreBd);
                if (Existe.ExecuteScalar() is not DBNull and not null)
                {
                    Log.LogInformation("Base de datos '{Bd}' encontrada en {Servidor}.", NombreBd, Datos.DataSource);
                    return false;
                }
            }

            var RutaScript = Path.Combine(AppContext.BaseDirectory, "BaseDatos", "ScriptCompleto.sql");
            Log.LogWarning("La base '{Bd}' no existe en {Servidor}. Creándola con {Script}...", NombreBd, Datos.DataSource, RutaScript);

            // El script usa el nombre TerraNovaDB; si el compañero eligio otro nombre, se reemplaza
            var Script = File.ReadAllText(RutaScript).Replace("TerraNovaDB", NombreBd);

            try
            {
                // SQL Server no entiende "GO": separa el script en lotes y los ejecuta uno por uno
                var Lotes = Regex.Split(Script, @"^\s*GO\s*;?\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                foreach (var Lote in Lotes.Where(L => !string.IsNullOrWhiteSpace(L)))
                {
                    using var Comando = new SqlCommand(Lote, Conexion) { CommandTimeout = 120 };
                    Comando.ExecuteNonQuery();
                }
            }
            catch
            {
                // Si fallo a medias, se borra la base que ACABAMOS de crear (no existia antes),
                // para no dejarla incompleta: en el proximo inicio se vuelve a intentar desde cero.
                using var Limpiar = new SqlCommand(
                    $"USE master; IF DB_ID(@Nombre) IS NOT NULL BEGIN " +
                    $"ALTER DATABASE [{NombreBd.Replace("]", "]]")}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                    $"DROP DATABASE [{NombreBd.Replace("]", "]]")}]; END", Conexion);
                Limpiar.Parameters.AddWithValue("@Nombre", NombreBd);
                Limpiar.ExecuteNonQuery();
                throw;
            }

            Log.LogWarning("Base de datos '{Bd}' creada con datos de prueba. Usuario: admin / Admin123!", NombreBd);
            return true;
        }
    }
}
