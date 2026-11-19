# Glosario TerraNova MAUI — 20 preguntas para la presentación

Respuestas cortas, con el ejemplo concreto del proyecto para poder mostrarlo en vivo.

---

## Arquitectura

### 1. ¿Qué es .NET MAUI y por qué lo usaron?
**.NET MAUI** (Multi-platform App UI) permite crear apps para **Android, iOS, Windows y macOS con un solo código** en C# y XAML.
Lo usamos porque la versión web de TerraNova ya estaba en C# (Blazor + ASP.NET Core): reutilizamos los mismos DTOs, la misma API y el mismo lenguaje.

### 2. ¿Cómo está organizada la solución?
En **3 proyectos**:
- `TerraNova.Shared` → DTOs (las "cajas" de datos que viajan entre la API y la app)
- `TerraNova.Api` → API REST con Entity Framework Core y SQL Server
- `TerraNova` → la app MAUI

### 3. ¿Por qué la app no se conecta directo a SQL Server?
Por **seguridad y buena práctica**: si la app tuviera la cadena de conexión, cualquiera podría extraerla del celular y entrar a la base.
El flujo es **App → API (HTTP/JSON) → SQL Server**. La API valida, aplica reglas (por ejemplo, al vender cambia el estado de la propiedad) y es la única que toca la base.

### 4. ¿Qué es MVVM y cómo lo aplicaron?
**Model–View–ViewModel**: separa la pantalla (**View**, XAML), la lógica (**ViewModel**, C#) y los datos (**Model**, DTOs).
Ejemplo: `ClientesPage.xaml` solo dibuja; `ClientesViewModel.cs` carga, filtra y elimina clientes. La View se comunica con el ViewModel mediante **bindings** y **commands**, sin código en el `.xaml.cs`.

### 5. ¿Qué es un Binding?
Es el "enlace" entre un control y una propiedad del ViewModel: `<Entry Text="{Binding Cliente.Nombre}"/>`.
Si el usuario escribe, se actualiza la propiedad; si la propiedad cambia, se actualiza la pantalla. Usamos **bindings compilados** (`x:DataType`), que son más rápidos y detectan errores al compilar.

### 6. ¿Qué es la Inyección de Dependencias?
En lugar de que cada clase cree lo que necesita (`new ServicioClientes()`), se **registra todo en `MauiProgram.cs`** y MAUI lo entrega por el constructor.
Ejemplo: `ClientesViewModel(ServicioClientes servicio)`. Así hay un solo `HttpClient` para toda la app y es fácil cambiar o probar piezas.

---

## Librerías

### 7. ¿Qué es CommunityToolkit.Maui y qué usaron de él?
Librería oficial de la comunidad .NET (respaldada por Microsoft) que agrega controles y utilidades a MAUI:
- **Toast**: avisos que desaparecen solos ("Cliente registrado correctamente")
- **Behaviors de validación**: `EmailValidationBehavior` y `TextValidationBehavior` (el texto se pone rojo mientras escribes un dato inválido)
- **AvatarView**: la foto o las iniciales de los trabajadores
- **Converters**: `InvertedBoolConverter`, `IsStringNotNullOrEmptyConverter`

### 8. ¿Y CommunityToolkit.Mvvm?
Genera automáticamente el código repetitivo de MVVM:
- `[ObservableProperty]` crea la propiedad que avisa a la pantalla cuando cambia
- `[RelayCommand]` convierte un método en un comando para los botones

Sin esto, cada propiedad necesitaría ~10 líneas de `INotifyPropertyChanged`.

### 9. ¿Por qué Mapsui y no Google Maps?
| | Mapsui | Google Maps (Microsoft.Maui.Controls.Maps) |
|---|---|---|
| Costo / API Key | Gratis, **sin API Key** | Requiere API Key de Google |
| Windows | **Sí** | No |
| Mapas | OpenStreetMap (los mismos que la web con Leaflet) | Google |

Se usa en 3 lugares: el **mapa general** (marcadores por color según el estado), el **detalle** y el **formulario** (tocas el mapa y se guardan latitud y longitud).

### 10. ¿Qué es Entity Framework Core?
Un **ORM**: permite trabajar la base de datos con clases de C# en vez de escribir SQL a mano.
`Contexto.Clientes.Where(c => c.Ci == ci)` se traduce solo a `SELECT ... WHERE CI = @ci`. Protege contra **inyección SQL** porque usa parámetros.

---

## Base de datos

### 11. ¿Por qué SQL Server?
El sistema original ya usaba SQL Server, con su script, sus restricciones (`UNIQUE` en CI, email y teléfono) y sus llaves foráneas. Mantenerlo permitió reutilizar la API sin cambios de modelo.
Como todo pasa por EF Core, migrar a PostgreSQL solo requeriría cambiar el paquete (`Npgsql`) y una línea (`UseNpgsql`).

### 12. ¿Cómo hace cada compañero para usar su propia base de datos?
- Cada uno pone **su** cadena de conexión en `appsettings.Local.json`, que **no se sube a GitHub**.
- La primera vez que corre la API, el `InicializadorBaseDatos` detecta que la base no existe y la **crea sola** con `ScriptCompleto.sql`.
- Si falla a mitad, la borra para no dejarla incompleta y vuelve a intentar en el siguiente inicio.

### 13. ¿Qué tablas tiene y cómo se relacionan?
**Trabajadores, Clientes, Propiedades, Ventas, Visitas y Usuarios.**
- Una **Propiedad** tiene un Trabajador (agente) asignado.
- Una **Venta** y una **Visita** unen Cliente + Propiedad + Trabajador.
- Un **Usuario** (login) puede estar vinculado a un Trabajador.

No se puede eliminar una propiedad que tiene ventas o visitas (integridad referencial).

### 14. ¿Cómo se guardan las imágenes y por qué no en la base de datos?
La foto se **comprime en el dispositivo** (máx. 1280 px, JPEG 80 %: una foto de 4 MB queda en ~150 KB), se sube a la API y se guarda como **archivo** en `wwwroot/imagenes/`. La base solo guarda la ruta (`imagenes/trabajadores/abc.jpg`).
Guardarla en base64 dentro de la tabla la haría pesada (cada foto ocuparía ~2,7 MB de texto) y cada lista de trabajadores arrastraría todas las fotos. La API además **verifica que sea una imagen real** (revisa los primeros bytes, no la extensión) y **borra el archivo viejo** cuando se cambia la foto.

---

## Seguridad y reglas de negocio

### 15. ¿Cómo se guardan las contraseñas?
Con **BCrypt** (un hash lento con "sal" aleatoria). La base nunca guarda la contraseña real; en el login se compara con `BCrypt.Verify`.
Aunque alguien robe la tabla Usuarios, no puede leer las contraseñas. La API **nunca devuelve el hash** en sus respuestas.

### 16. ¿Qué diferencia hay entre Admin y Agente?
| Acción | Admin | Agente |
|---|---|---|
| Ver menú **Usuarios** | Sí | No |
| Editar propiedades | Todas | Solo las suyas |
| Eliminar propiedades | Sí | No |
| Elegir el agente de una visita o venta | Sí | No: queda **a su nombre automáticamente** |
| Filtro "solo mis propiedades" | — | Sí |

### 17. ¿Qué pasa cuando se registra una venta?
1. Al elegir la propiedad, el **monto** toma su precio.
2. Al elegir el agente, se calcula la **comisión automáticamente**: `monto × % de comisión del agente`.
3. Al guardar, la API (dentro de una **transacción**) registra la venta y cambia la propiedad a **Vendida**, o a **Alquilada** si la forma de pago es Alquiler.
4. Si se anula la venta, la propiedad vuelve a **Disponible**.

---

## Experiencia de usuario

### 18. ¿Cómo se adapta la app a celular y a Windows?
Con `OnIdiom` en XAML: el mismo código cambia según el dispositivo.
- **Windows**: menú lateral fijo, listas en 2 columnas, formularios centrados, efecto *hover*, barra de título verde.
- **Celular**: menú desplegable (☰), listas en 1 columna.

Los íconos son **Fluent UI System Icons** (los de Windows 11) cargados como fuente, así se ven nítidos en cualquier tamaño.

### 19. ¿Cómo manejan los decimales si en Bolivia se usa coma?
Windows en **es-BO** usa la coma (`3,5`), pero mucha gente escribe con punto (`3.5`).
La clase `Helpers/Numeros.cs` **acepta ambos** en la comisión, el precio, el área y el monto, y la validación visual (`TextValidationBehavior` con una expresión regular) marca en rojo solo lo que realmente es inválido.

### 20. ¿Qué mejorarían si tuvieran más tiempo?
- **Autenticación con token (JWT)** en la API: hoy cualquiera que conozca la URL podría llamarla.
- **Modo sin conexión**: guardar datos en SQLite y sincronizar después.
- **Notificaciones push** para recordar las visitas programadas.
- **Pruebas unitarias** de los ViewModels.
- Publicar la API en la nube (Azure) y guardar las imágenes en **Azure Blob Storage**.
