using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TerraNova.Api.Models;

namespace TerraNova.Api.Services
{
    // Genera el token JWT que la app envia en cada peticion (cabecera "Authorization: Bearer ...").
    // El token lleva quien es el usuario, su rol y su trabajador: la API decide los permisos con eso,
    // asi no se puede "saltar" la seguridad llamando a la API desde fuera de la app.
    public class ServicioToken
    {
        public const string ClaimIdTrabajador = "idTrabajador";
        public const string Emisor = "TerraNova.Api";

        public SymmetricSecurityKey Clave { get; }
        private readonly int HorasValidez;

        public ServicioToken(IConfiguration Config, ILogger<ServicioToken> Log)
        {
            // La clave va en appsettings.Local.json ("Jwt:Clave"), que no se sube a GitHub.
            // Si no hay, se genera una al azar en cada inicio: funciona, pero al reiniciar la API hay que volver a iniciar sesion.
            var Texto = Config["Jwt:Clave"];
            // (la clave de ejemplo del repositorio es publica: no se acepta)
            if (string.IsNullOrWhiteSpace(Texto) || Texto.Length < 32 || Texto.StartsWith("CAMBIE-ESTO"))
            {
                Texto = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
                Log.LogWarning("Jwt:Clave no configurada: se usa una clave temporal (las sesiones se cierran al reiniciar la API).");
            }
            Clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Texto));
            HorasValidez = int.TryParse(Config["Jwt:HorasValidez"], out var H) ? H : 12;
        }

        public string Generar(Usuario U)
        {
            var Claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, U.Id.ToString()),
                new(ClaimTypes.Name, U.NombreUsuario),
                new(ClaimTypes.Role, U.Rol)
            };
            if (U.IdTrabajador.HasValue)
                Claims.Add(new Claim(ClaimIdTrabajador, U.IdTrabajador.Value.ToString()));

            var Token = new JwtSecurityToken(
                issuer: Emisor,
                audience: Emisor,
                claims: Claims,
                expires: DateTime.UtcNow.AddHours(HorasValidez),
                signingCredentials: new SigningCredentials(Clave, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(Token);
        }
    }
}
