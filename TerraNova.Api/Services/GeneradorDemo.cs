using TerraNova.Api.Models;

namespace TerraNova.Api.Services
{
    // Genera datos de demostracion realistas (Santa Cruz de la Sierra) para que la app tenga "vida":
    // agentes, clientes, propiedades con foto y ubicacion, ventas con su comision y visitas pasadas/futuras.
    //
    // - Respeta las mismas reglas que la API: una venta marca la propiedad como Vendida/Alquilada
    //   y la comision se calcula con el % del agente.
    // - Usa una semilla fija: todos los compañeros obtienen exactamente los mismos datos.
    // - Se ejecuta UNA sola vez: si ya existe el primer agente demo (Marcador), no hace nada.
    public static class GeneradorDemo
    {
        public const string DominioAgentes = "@terranova.bo";
        public const string ClaveAgentes = "Agente123!";

        // Email del primer agente que crea el generador: si existe, los datos demo ya se cargaron
        public const string Marcador = "lucia.gutierrez" + DominioAgentes;

        public record Resumen(bool YaExistia, int Trabajadores, int Clientes, int Propiedades, int Ventas, int Visitas,
                              int Usuarios, int FotosCompletadas);

        // ===== Catalogos =====
        private record Zona(string Nombre, double Lat, double Lng);

        private static readonly Zona[] Zonas =
        {
            new("Equipetrol", -17.7650, -63.1975), new("Urubó", -17.7550, -63.2420), new("Las Palmas", -17.8000, -63.2050),
            new("Sirari", -17.7690, -63.1900), new("Centro", -17.7835, -63.1820), new("Norte", -17.7450, -63.1760),
            new("Las Brisas", -17.7600, -63.1700), new("Hamacas", -17.7540, -63.1640), new("Sur", -17.8200, -63.1820),
            new("Villa 1ro de Mayo", -17.7720, -63.1320), new("Plan 3000", -17.8300, -63.1330), new("Av. Banzer", -17.7300, -63.1680),
            new("Remanso", -17.7930, -63.2150), new("Barrio Urbari", -17.8050, -63.1930)
        };

        private static readonly string[] Nombres = { "Lucía", "Carlos", "Valeria", "Diego", "Camila", "Jorge", "Mariana", "Luis",
            "Sofía", "Andrés", "Gabriela", "Miguel", "Paola", "José", "Daniela", "Rodrigo", "Natalia", "Fernando", "Alejandra",
            "Marcelo", "Carla", "Javier", "Verónica", "Ricardo", "Andrea", "Sergio", "Fabiola", "Óscar", "Mónica", "Raúl" };

        private static readonly string[] Apellidos = { "Gutiérrez", "Rojas", "Flores", "Vargas", "Mendoza", "Suárez", "Justiniano",
            "Montaño", "Pinto", "Saucedo", "Áñez", "Rivero", "Cuéllar", "Méndez", "Paz", "Roca", "Terrazas", "Salvatierra",
            "Moreno", "Ortiz", "Arteaga", "Chávez", "Peña", "Banegas", "Durán", "Hurtado", "Callaú", "Ribera", "Melgar", "Zeballos" };

        private static readonly string[] Calles = { "Av. San Martín", "Calle Los Pinos", "Av. Beni", "Av. Alemana", "Calle Libertad",
            "Av. Grigotá", "Av. Cristo Redentor", "Calle Las Begonias", "Av. Mutualista", "Calle Ñuflo de Chávez", "Av. Piraí",
            "Av. Paraguá", "Calle Independencia", "Av. Roca y Coronado", "Calle Sucre" };

        private const string Unsplash = "https://images.unsplash.com/";
        private const string Tamano = "?w=1000&q=80&fit=crop";

        private static readonly Dictionary<string, string[]> Fotos = new()
        {
            ["Casa"] = new[] { "photo-1568605114967-8130f3a36994", "photo-1570129477492-45c003edd2be", "photo-1600596542815-ffad4c1539a9",
                "photo-1600585154340-be6161a56a0c", "photo-1512917774080-9991f1c4c750", "photo-1564013799919-ab600027ffc6",
                "photo-1580587771525-78b9dba3b914", "photo-1613490493576-7fde63acd811", "photo-1605276374104-dee2a0ed3cd6",
                "photo-1416331108676-a22ccb276e35", "photo-1600607687939-ce8a6c25118c", "photo-1449844908441-8829872d2607",
                "photo-1592595896551-12b371d546d5" },
            ["Departamento"] = new[] { "photo-1502672260266-1c1ef2d93688", "photo-1522708323590-d24dbb6b0267", "photo-1560448204-e02f11c3d0e2",
                "photo-1545324418-cc1a3fa10c00", "photo-1493809842364-78817add7ffb", "photo-1554995207-c18c203602cb",
                "photo-1556912167-f556f1f39fdf", "photo-1501183638710-841dd1904471", "photo-1595526114035-0d45ed16cfbf" },
            ["Oficina"] = new[] { "photo-1497366811353-6870744d04b2", "photo-1497215842964-222b430dc094", "photo-1524758631624-e2822e304c36" },
            ["Local"] = new[] { "photo-1497366216548-37526070297c", "photo-1441986300917-64674bd600d8" },
            ["Terreno"] = new[] { "photo-1500382017468-9049fed747ef", "photo-1516156008625-3a9d6067fab5" }
        };

        private static readonly Dictionary<string, string[]> Titulos = new()
        {
            ["Casa"] = new[] { "Casa familiar", "Casa con piscina", "Casa moderna", "Casa de dos plantas", "Casa en condominio", "Residencia" },
            ["Departamento"] = new[] { "Departamento amoblado", "Departamento con vista", "Monoambiente", "Departamento de 2 dormitorios", "Penthouse" },
            ["Oficina"] = new[] { "Oficina ejecutiva", "Oficina en torre empresarial", "Oficina amoblada" },
            ["Local"] = new[] { "Local comercial", "Local sobre avenida", "Tienda en galería" },
            ["Terreno"] = new[] { "Terreno urbanizado", "Lote en condominio", "Terreno esquina" }
        };

        private static readonly string[] Detalles = { "Excelente iluminación natural.", "Cerca de colegios, supermercados y centros de salud.",
            "Zona tranquila y segura.", "Acabados de primera.", "Ideal para inversión.", "Lista para habitar.", "Documentación al día.",
            "Fácil acceso a avenidas principales.", "Áreas verdes y seguridad 24 horas.", "Posibilidad de financiamiento bancario." };

        private static readonly string[] ComentariosVisita = { "Cliente muy interesado, pidió segunda visita.", "Le pareció caro, evaluará opciones.",
            "Consultó por financiamiento bancario.", "Vino con su familia, les gustó la zona.", "Pidió fotos adicionales por WhatsApp.",
            "Busca algo con más habitaciones.", "Interesado para inversión.", "Quiere negociar el precio.", "Primera visita, buena impresión.",
            "Consultó por los gastos de expensas." };

        // ===== Generacion =====
        public static Resumen Generar(TerraNovaDbContext Contexto, bool CompletarFotos)
        {
            if (Contexto.Trabajadores.Any(T => T.Email == Marcador))
                return new Resumen(true, 0, 0, 0, 0, 0, 0, 0);

            var Azar = new Random(2026);
            var Hoy = DateTime.Today;
            T Elegir<T>(IList<T> Lista) => Lista[Azar.Next(Lista.Count)];

            // Valores unicos ya usados (CI, email y telefono tienen restriccion UNIQUE)
            var CisUsados = Contexto.Trabajadores.Select(T => T.Ci).Concat(Contexto.Clientes.Select(C => C.Ci)).ToHashSet();
            var EmailsUsados = Contexto.Trabajadores.Select(T => T.Email).Concat(Contexto.Clientes.Select(C => C.Email)).ToHashSet();
            var TelefonosUsados = Contexto.Clientes.Select(C => C.Telefono).ToHashSet();
            var UsuariosUsados = Contexto.Usuarios.Select(U => U.NombreUsuario).ToHashSet();

            string NuevoCi() { string Ci; do Ci = Azar.Next(3_000_000, 13_000_000).ToString(); while (!CisUsados.Add(Ci)); return Ci; }
            string NuevoTelefono() { string T; do T = $"{Elegir(new[] { 6, 7 })}{Azar.Next(0, 10_000_000):D7}"; while (!TelefonosUsados.Add(T)); return T; }
            string NuevoEmail(string Nombre, string Apellido, string Dominio)
            {
                var Base = $"{Simple(Nombre)}.{Simple(Apellido)}";
                var Email = Base + Dominio;
                for (var N = 2; !EmailsUsados.Add(Email); N++) Email = $"{Base}{N}{Dominio}";
                return Email;
            }

            // ----- Trabajadores: 6 agentes + 1 administrador + 1 recepcionista -----
            var Cargos = new[] { "Agente", "Agente", "Agente", "Agente", "Agente", "Agente", "Administrador", "Recepcionista" };
            var NuevosTrabajadores = Cargos.Select((Cargo, I) =>
            {
                var Nombre = Nombres[I]; var Apellido = Apellidos[I];
                return new Trabajador
                {
                    Nombre = Nombre, Apellido = Apellido, Ci = NuevoCi(), Cargo = Cargo,
                    Telefono = $"7{Azar.Next(0, 10_000_000):D7}", Email = NuevoEmail(Nombre, Apellido, DominioAgentes),
                    PorcentajeComision = Cargo == "Agente" ? Math.Round(2.5m + (decimal)Azar.NextDouble() * 2.5m, 2) : 0,
                    FechaContratacion = DateOnly.FromDateTime(Hoy.AddDays(-Azar.Next(120, 1500))),
                    FechaNacimiento = DateOnly.FromDateTime(Hoy.AddYears(-Azar.Next(24, 52)).AddDays(-Azar.Next(0, 365))),
                    Activo = true
                };
            }).ToList();
            Contexto.Trabajadores.AddRange(NuevosTrabajadores);
            Contexto.SaveChanges();

            // Agentes disponibles para asignar propiedades (los nuevos y los que ya existian)
            var Agentes = Contexto.Trabajadores.Where(T => T.Activo && T.Cargo == "Agente").ToList();

            // ----- Usuarios de acceso para 3 agentes nuevos (para probar el rol Agente) -----
            var Hash = BCrypt.Net.BCrypt.HashPassword(ClaveAgentes);
            var NuevosUsuarios = NuevosTrabajadores.Where(T => T.Cargo == "Agente").Take(3)
                .Select(T => new Usuario { NombreUsuario = Simple(T.Nombre), ContraseñaHash = Hash, Rol = "Agente", IdTrabajador = T.Id, Activo = true })
                .Where(U => UsuariosUsados.Add(U.NombreUsuario)).ToList();
            Contexto.Usuarios.AddRange(NuevosUsuarios);

            // ----- Clientes -----
            var TiposCliente = new[] { "Comprador", "Comprador", "Comprador", "Vendedor", "Ambos" };
            var NuevosClientes = Enumerable.Range(0, 60).Select(_ =>
            {
                var Nombre = Elegir(Nombres); var Apellido = Elegir(Apellidos);
                return new Cliente
                {
                    Nombre = Nombre, Apellido = $"{Apellido} {Elegir(Apellidos)}", Ci = NuevoCi(), Telefono = NuevoTelefono(),
                    Email = NuevoEmail(Nombre, Apellido, "@gmail.com"),
                    Direccion = $"{Elegir(Calles)} #{Azar.Next(10, 2000)}, {Elegir(Zonas).Nombre}",
                    FechaRegistro = Hoy.AddDays(-Azar.Next(1, 420)).AddHours(Azar.Next(8, 19)),
                    TipoCliente = Elegir(TiposCliente)
                };
            }).ToList();
            Contexto.Clientes.AddRange(NuevosClientes);
            Contexto.SaveChanges();
            var Clientes = Contexto.Clientes.ToList();

            // ----- Propiedades -----
            var Tipos = new[] { "Casa", "Casa", "Casa", "Departamento", "Departamento", "Departamento", "Oficina", "Local", "Terreno" };
            var NuevasPropiedades = Enumerable.Range(0, 60).Select(I =>
            {
                var Tipo = Elegir(Tipos);
                var Zona = Elegir(Zonas);
                var Alquiler = Tipo != "Terreno" && Azar.NextDouble() < 0.3;
                var (Precio, Area, Hab, Ban, Gar) = Caracteristicas(Tipo, Alquiler, Azar);
                return new Propiedad
                {
                    Titulo = $"{Elegir(Titulos[Tipo])} en {Zona.Nombre}",
                    Tipo = Tipo,
                    Direccion = $"{Elegir(Calles)} #{Azar.Next(10, 3000)}",
                    Zona = Zona.Nombre,
                    Precio = Precio, AreaM2 = Area, Habitaciones = Hab, Banios = Ban, Garajes = Gar,
                    Estado = "Disponible",
                    TipoOperacion = Alquiler ? "Alquiler" : "Venta",
                    Descripcion = $"{Tipo} de {Area:N0} m² en {Zona.Nombre}. " + string.Join(" ", Detalles.OrderBy(_ => Azar.Next()).Take(3)),
                    ImagenUrl = Unsplash + Fotos[Tipo][I % Fotos[Tipo].Length] + Tamano,
                    IdTrabajador = Elegir(Agentes).Id,
                    Latitud = Math.Round(Zona.Lat + (Azar.NextDouble() - 0.5) * 0.016, 6),
                    Longitud = Math.Round(Zona.Lng + (Azar.NextDouble() - 0.5) * 0.016, 6),
                    FechaPublicacion = Hoy.AddDays(-Azar.Next(15, 420)).AddHours(Azar.Next(8, 19))
                };
            }).ToList();
            Contexto.Propiedades.AddRange(NuevasPropiedades);
            Contexto.SaveChanges();

            // ----- Ventas: ~1 de cada 3 propiedades nuevas se vendio o alquilo -----
            var NuevasVentas = new List<Venta>();
            foreach (var P in NuevasPropiedades.Where(_ => Azar.NextDouble() < 0.33))
            {
                var Agente = Agentes.First(A => A.Id == P.IdTrabajador);
                var EsAlquiler = P.TipoOperacion == "Alquiler";
                var Fecha = P.FechaPublicacion.AddDays(Azar.Next(10, Math.Max(11, (Hoy - P.FechaPublicacion).Days)));
                if (Fecha > Hoy) Fecha = Hoy.AddDays(-Azar.Next(0, 5));
                var Monto = EsAlquiler ? P.Precio : Math.Round(P.Precio * (decimal)(0.90 + Azar.NextDouble() * 0.10) / 500) * 500;
                var Pendiente = Azar.NextDouble() < 0.12;

                NuevasVentas.Add(new Venta
                {
                    IdCliente = Elegir(Clientes).Id, IdPropiedad = P.Id, IdTrabajador = Agente.Id,
                    Fecha = Fecha.Date.AddHours(Azar.Next(9, 18)),
                    Monto = Monto,
                    ComisionGenerada = decimal.Round(Monto * Agente.PorcentajeComision / 100m, 2),
                    FormaPago = EsAlquiler ? "Alquiler" : Elegir(new[] { "Contado", "Crédito", "Financiado" }),
                    Estado = Pendiente ? "Pendiente" : "Completada",
                    Observaciones = EsAlquiler ? "Contrato de alquiler por 12 meses." : Elegir(new[] {
                        "Pago con crédito hipotecario.", "Firma de minuta realizada.", "Cliente referido por otro comprador.", "Negociación cerrada en segunda visita." })
                });
                // Misma regla que VentaController: la propiedad cambia de estado
                P.Estado = Pendiente ? "Reservada" : EsAlquiler ? "Alquilada" : "Vendida";
            }
            Contexto.Ventas.AddRange(NuevasVentas);

            // ----- Visitas: historial + agenda de los proximos dias -----
            var NuevasVisitas = new List<Visita>();
            var VentaDe = NuevasVentas.ToDictionary(V => V.IdPropiedad);
            foreach (var P in NuevasPropiedades)
            {
                var Cantidad = Azar.Next(0, 5);
                for (var N = 0; N < Cantidad; N++)
                {
                    var Limite = VentaDe.TryGetValue(P.Id, out var V) ? V.Fecha : Hoy;
                    var Dias = Math.Max(1, (Limite - P.FechaPublicacion).Days);
                    NuevasVisitas.Add(new Visita
                    {
                        IdCliente = Elegir(Clientes).Id, IdPropiedad = P.Id, IdTrabajador = P.IdTrabajador,
                        FechaVisita = P.FechaPublicacion.Date.AddDays(Azar.Next(1, Dias + 1)).AddHours(Azar.Next(9, 19)),
                        Estado = Azar.NextDouble() < 0.85 ? "Realizada" : "Cancelada",
                        Comentarios = Elegir(ComentariosVisita)
                    });
                }
                // Las propiedades que siguen disponibles tienen visitas agendadas
                if (P.Estado == "Disponible" && Azar.NextDouble() < 0.5)
                {
                    NuevasVisitas.Add(new Visita
                    {
                        IdCliente = Elegir(Clientes).Id, IdPropiedad = P.Id, IdTrabajador = P.IdTrabajador,
                        FechaVisita = Hoy.AddDays(Azar.Next(0, 21)).AddHours(Azar.Next(9, 19)).AddMinutes(Elegir(new[] { 0, 30 })),
                        Estado = "Programada",
                        Comentarios = Elegir(new[] { "Primera visita.", "Segunda visita con su pareja.", "Viene a ver acabados.", "Cliente referido." })
                    });
                }
            }
            Contexto.Visitas.AddRange(NuevasVisitas);

            // ----- Fotos para propiedades existentes sin imagen (o con links de Drive, que no se pueden mostrar) -----
            var FotosCompletadas = 0;
            if (CompletarFotos)
            {
                foreach (var P in Contexto.Propiedades.Where(P => P.ImagenUrl == null || P.ImagenUrl == "" || P.ImagenUrl.Contains("drive.google.com")).ToList())
                {
                    var Lista = Fotos.TryGetValue(P.Tipo, out var F) ? F : Fotos["Casa"];
                    P.ImagenUrl = Unsplash + Lista[P.Id % Lista.Length] + Tamano;
                    FotosCompletadas++;
                }
            }

            Contexto.SaveChanges();
            return new Resumen(false, NuevosTrabajadores.Count, NuevosClientes.Count, NuevasPropiedades.Count,
                               NuevasVentas.Count, NuevasVisitas.Count, NuevosUsuarios.Count, FotosCompletadas);
        }

        private static (decimal Precio, double Area, int Hab, int Ban, int Gar) Caracteristicas(string Tipo, bool Alquiler, Random Azar)
        {
            decimal Venta(int Min, int Max) => Math.Round((decimal)Azar.Next(Min, Max) / 500) * 500;
            decimal Renta(int Min, int Max) => Math.Round((decimal)Azar.Next(Min, Max) / 10) * 10;
            return Tipo switch
            {
                "Casa" => (Alquiler ? Renta(600, 2500) : Venta(90_000, 450_000), Azar.Next(160, 600), Azar.Next(2, 6), Azar.Next(2, 5), Azar.Next(1, 4)),
                "Departamento" => (Alquiler ? Renta(350, 1200) : Venta(55_000, 220_000), Azar.Next(45, 180), Azar.Next(1, 4), Azar.Next(1, 3), Azar.Next(0, 2)),
                "Oficina" => (Alquiler ? Renta(500, 2000) : Venta(70_000, 300_000), Azar.Next(40, 250), 0, Azar.Next(1, 3), Azar.Next(0, 3)),
                "Local" => (Alquiler ? Renta(400, 1800) : Venta(60_000, 250_000), Azar.Next(30, 200), 0, 1, Azar.Next(0, 2)),
                _ => (Venta(25_000, 180_000), Azar.Next(300, 2000), 0, 0, 0)
            };
        }

        // "Lucía" -> "lucia" (para emails y nombres de usuario)
        private static string Simple(string Texto) =>
            new string(Texto.ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD)
                .Where(C => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(C) != System.Globalization.UnicodeCategory.NonSpacingMark && C != ' ')
                .ToArray()).Replace("ñ", "n");
    }
}
