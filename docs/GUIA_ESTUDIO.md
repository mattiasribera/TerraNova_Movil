# Guía de estudio — TerraNova MAUI

Para preparar la defensa: qué decir, cómo funciona cada parte y **dónde está en el código** para mostrarlo.
Complementa a [`GLOSARIO.md`](GLOSARIO.md) (20 preguntas cortas) y [`MAPAS.md`](MAPAS.md) (todo sobre los mapas).

---

## 1. Presentación en 1 minuto (apréndetela)

> TerraNova es un sistema para una inmobiliaria de Santa Cruz. Llevamos la versión web (Blazor) a **.NET MAUI**,
> así que la misma app corre en **Android y Windows** con un solo código en C#.
>
> La solución tiene **3 proyectos**: la **app MAUI** con el patrón **MVVM**, una **API REST** en ASP.NET Core con
> **Entity Framework Core** y **SQL Server**, y un proyecto **Shared** con los DTOs que usan los dos.
>
> La app **nunca toca la base de datos**: todo pasa por la API, que valida los **permisos por rol con JWT**
> (un agente solo puede editar lo suyo). Usamos **Mapsui** para mapas sin API Key, **ECharts** para los gráficos,
> y **CommunityToolkit** para validaciones, avisos y MVVM.

---

## 2. Arquitectura (dibújala en la pizarra si te la piden)

```
 ┌──────────────── App .NET MAUI (Android / Windows) ────────────────┐
 │  View (XAML)  ⇄ binding ⇄  ViewModel (C#)  →  Service (HttpClient) │
 │  ClientesPage.xaml          ClientesViewModel    ServicioClientes   │
 └──────────────────────────────────────┬─────────────────────────────┘
                                        │ HTTP + JSON   (Authorization: Bearer <token JWT>)
 ┌──────────────── TerraNova.Api (ASP.NET Core) ──────────────────────┐
 │  Controller  →  permisos (rol / dueño)  →  EF Core (DbContext)       │
 │  ClienteController                          TerraNovaDbContext       │
 └──────────────────────────────────────┬─────────────────────────────┘
                                        │ SQL
                              ┌─────────▼─────────┐
                              │ SQL Server         │   + archivos de fotos en
                              │ TerraNovaDB        │     wwwroot/imagenes/
                              └────────────────────┘
        TerraNova.Shared: DTOs (ClienteDTO, VentaDTO, ResponseAPI...) usados por la App y la API
```

**Frase clave:** *"Separamos responsabilidades: la app muestra, la API decide y valida, la base guarda."*

---

## 3. "¿Qué pasa cuando...?" — recorridos paso a paso

### 3.1 Inicio de sesión
1. `Views/Login/LoginPage.xaml` — el usuario escribe; el campo contraseña es `Controls/CampoContrasena.cs` (con el "ojo").
2. `ViewModels/Login/LoginViewModel.cs` → `IniciarSesionCommand` llama a `ServicioAutenticacion.Login`.
3. `Services/ServicioAutenticacion.cs` hace `POST api/Usuario/Login`.
4. API `Controllers/UsuarioController.cs` → `Login`: busca el usuario activo y compara con **`BCrypt.Verify`**.
5. Si es correcto, `Services/ServicioToken.cs` genera un **token JWT** con el id, el **rol** y el **idTrabajador**.
6. La app guarda el usuario + token en `Preferences` y agrega `Authorization: Bearer <token>` a **todas** las peticiones.
7. `AppShell.ActualizarMenu()` oculta "Usuarios" si no es Admin, y navega a `//Inicio`.

### 3.2 Ver la lista de clientes
1. Al aparecer la página, `Views/Base/PaginaBase.cs` → `OnAppearing` llama a `Cargar()` del ViewModel (interfaz `ICargable`).
2. `ClientesViewModel.Cargar` → `ServicioClientes.Lista()` → `GET api/Cliente/Lista`.
3. `Services/Api/ServicioBase.cs` lee el `ResponseAPI<T>` y maneja los errores (401, 403, sin conexión).
4. Los datos van a un `ObservableCollection<ClienteDTO>`: al agregar o quitar elementos, **la pantalla se actualiza sola**.
5. La búsqueda: `TextoBusqueda` es `[ObservableProperty]`; `OnTextoBusquedaChanged` llama a `Filtrar()` en cada tecla.

### 3.3 Registrar una venta (la regla de negocio más completa)
1. `ViewModels/Ventas/VentaFormViewModel.cs`: al elegir propiedad, el **monto = precio**; al elegir agente,
   **comisión = monto × % del agente** (`CalcularComision`). Si quien entra es un agente, queda a su nombre.
2. `POST api/Venta/Guardar` → `VentaController.Guardar`:
   - Si no es Admin, **fuerza** `IdTrabajador` = el del token (no se puede registrar a nombre de otro).
   - Usa una **transacción**: guarda la venta **y** cambia la propiedad a "Vendida" (o "Alquilada").
     Si algo falla, se deshace todo (no queda una venta sin su propiedad actualizada).
3. Si se **anula** (solo Admin), la propiedad vuelve a "Disponible", también en transacción.

### 3.4 Subir una foto
1. `Services/ServicioImagenes.cs`: `MediaPicker` (galería o cámara) → **SkiaSharp** la achica a 1280 px y la comprime a JPEG 80 %.
2. `POST api/Imagen/Subir/{carpeta}` (multipart) → `TerraNova.Api/Services/ServicioImagenes.cs`:
   revisa la **firma real del archivo** (los primeros bytes), no la extensión, y la guarda en `wwwroot/imagenes/`.
3. La base guarda **solo la ruta** (`imagenes/trabajadores/abc.jpg`). Al cambiar la foto, la API **borra la vieja**.
4. Para mostrarla: `Converters/FotoAImagenConverter.cs` la descarga y la guarda en memoria (caché).

### 3.5 Primera ejecución en una PC nueva
1. `TerraNova.Api/Program.cs` llama a `InicializadorBaseDatos.Ejecutar`.
2. Si `TerraNovaDB` no existe, ejecuta `BaseDatos/ScriptCompleto.sql` separando los lotes por `GO`.
3. Luego `GeneradorDemo` crea datos realistas (semilla fija: todos obtienen los mismos).
4. Si falla a mitad, **borra la base incompleta** para reintentar limpio.
5. Cada uno pone su cadena de conexión en `appsettings.Local.json` (no se sube a GitHub).

### 3.6 Un agente intenta editar la venta de otro
1. La app ni siquiera le muestra esas ventas (la API `Venta/Lista` **filtra por su idTrabajador**).
2. Si igual lo intenta (Postman, Swagger), `VentaController.Modificar` compara el dueño con el `idTrabajador` del token
   → responde **403 "Solo puede modificar sus propias ventas"**. Reglas en `TerraNova.Api/Services/Permisos.cs`.

---

## 4. Conceptos clave (definición corta + dónde está)

| Concepto | Explicación en una frase | Dónde mostrarlo |
|---|---|---|
| **MVVM** | La View (XAML) muestra, el ViewModel tiene la lógica, se conectan por binding | `ClientesPage.xaml` + `ClientesViewModel.cs` |
| **Binding** | Enlace automático entre un control y una propiedad | `Text="{Binding Cliente.Nombre}"` |
| **Binding compilado** | `x:DataType` hace que el binding se valide al compilar y sea más rápido | Arriba de cada página: `x:DataType="vm:..."` |
| **Command** | Un método del ViewModel que se ejecuta desde un botón | `[RelayCommand] Guardar()` → `GuardarCommand` |
| **ObservableProperty** | Propiedad que avisa a la pantalla cuando cambia | `[ObservableProperty] public partial string Monto` |
| **Inyección de dependencias** | MAUI crea y entrega los servicios por el constructor | `MauiProgram.cs` → `AddSingleton/AddTransient` |
| **Singleton vs Transient** | Singleton: una sola instancia (servicios). Transient: nueva cada vez (páginas, ViewModels) | `MauiProgram.cs` |
| **Shell** | Navegación de la app: menú lateral y rutas | `AppShell.xaml`, `Constantes/Rutas.cs` |
| **Navegación con parámetros** | `GoToAsync("ClienteForm?Id=5")`; el ViewModel lo recibe con `IQueryAttributable` | `FormularioViewModel.ApplyQueryAttributes` |
| **Converter** | Transforma un dato para mostrarlo (estado → color) | `Converters/EstadoAColorConverter.cs` |
| **Behavior** | Agrega comportamiento a un control sin modificarlo (validar mientras escribes) | `EmailValidationBehavior` en `ClienteFormPage.xaml` |
| **OnIdiom** | Valores distintos según dispositivo (celular / escritorio) | `Span="{OnIdiom Desktop=2, Default=1}"` |
| **DTO** | Objeto simple para transportar datos entre la API y la app | `TerraNova.Shared/*.cs` |
| **ResponseAPI<T>** | Envoltorio estándar: `EsCorrecto`, `Valor`, `Mensaje` | `TerraNova.Shared/ResponseAPI.cs` |
| **ORM (EF Core)** | Trabajar la base con clases C# en vez de SQL; usa parámetros (evita inyección SQL) | `TerraNovaDbContext.cs`, controladores |
| **Transacción** | Varias operaciones que se guardan juntas o ninguna | `VentaController.Guardar` |
| **JWT** | Token firmado que dice quién eres y tu rol; la API lo verifica en cada petición | `ServicioToken.cs`, `Program.cs` |
| **BCrypt** | Hash lento con "sal" para guardar contraseñas | `UsuarioController` (`HashPassword` / `Verify`) |
| **HybridWebView** | Muestra una página web local dentro de la app y se comunica con C# | `Controls/GraficoECharts.cs` |
| **adb reverse** | Por cable USB, el `localhost:5150` del celular apunta a la PC | `Services/Configuracion.cs` |

---

## 5. Preguntas difíciles (las que suelen hacer los ingenieros)

**¿Por qué MVVM y no MVC?**
Porque MAUI (XAML) está diseñado para *data binding*: la vista se actualiza sola cuando cambia el ViewModel, y el
ViewModel no conoce la pantalla, así que se puede probar sin interfaz. La **API** sí usa controladores al estilo MVC,
pero devuelve JSON en vez de vistas.

**¿Qué pasa si se cae la API?**
La app muestra *"No se pudo conectar con el servidor de TerraNova"* (en `ServicioBase.Enviar`). Si no hay internet,
dice *"No hay conexión a internet"*. No se cierra ni muestra errores técnicos.

**¿Qué pasa si alguien roba el token?**
Puede usarlo hasta que expire (12 horas). Por eso el token **vence** y está **firmado**: no se puede modificar
(por ejemplo, cambiar el rol a Admin) sin la clave secreta, que está solo en el servidor. Como mejora:
tokens más cortos + *refresh token*, y HTTPS obligatorio en producción.

**¿Por qué validan en la app Y en la API?**
En la app es por **comodidad** (aviso inmediato, texto en rojo). En la API es por **seguridad**: la app se puede
saltar llamando a la API directamente. **La validación que importa es la del servidor.**

**¿Por qué guardaron las fotos como archivo y no en la base de datos?**
En base64 cada foto pesaría ~2,7 MB de texto en la tabla, y cada consulta de la lista las arrastraría todas.
Como archivo, la base guarda ~60 caracteres y la foto se descarga solo cuando se necesita.

**¿Por qué SQL Server y no PostgreSQL?**
El sistema original ya estaba en SQL Server (script, restricciones, datos). Gracias a EF Core, cambiar de motor
es cambiar un paquete (`Npgsql`) y una línea (`UseNpgsql`). Los controladores no cambian.

**¿Cómo hacen que funcione en Android y Windows con el mismo código?**
MAUI tiene un solo proyecto con *targets* `net10.0-android` y `net10.0-windows`. Lo específico de cada plataforma
está en `Platforms/` (permisos de cámara en el `AndroidManifest.xml`). El diseño se adapta con `OnIdiom`.

**¿Cómo se comunica ECharts (JavaScript) con C#?**
`GraficoECharts` carga `Resources/Raw/graficos/index.html` en un `HybridWebView`. La página avisa "listo" con
`SendRawMessage`, y C# le envía las opciones con `EvaluateJavaScriptAsync("dibujar({...})")`.
Las opciones (JSON) se arman en `Helpers/OpcionesGraficos.cs`. ECharts va dentro de la app: no necesita internet.

**¿Qué es una inyección SQL y cómo la evitan?**
Meter código SQL en un campo de texto para que la base lo ejecute. EF Core siempre usa **consultas parametrizadas**:
el texto del usuario nunca se ejecuta como SQL.

**¿Cómo evitan que un agente se suba la comisión?**
`TrabajadorController.Modificar`: si no es Admin, la API **ignora** la comisión, el cargo y el estado que llegan,
y deja los valores que ya estaban en la base. En la app esos campos además aparecen bloqueados.

**¿Qué pasa si dos personas registran el mismo cliente?**
La base tiene restricciones **UNIQUE** en CI, email y teléfono. La app además consulta antes de guardar
(`ExisteCi`, `ExisteEmail`, `ExisteTelefono`) para mostrar un mensaje claro.

**¿Por qué una visita "Programada" no puede ser en el pasado, pero una "Realizada" sí?**
Programar es planificar a futuro. Realizada o Cancelada es registrar algo que ya ocurrió. Se valida en la app
(`VisitaFormViewModel.Guardar`) y en la API (`VisitaController`, responde 400).

**¿Cómo funcionan los decimales con coma y punto?**
En Bolivia (es-BO) se usa la coma, pero mucha gente escribe con punto. `Helpers/Numeros.cs` acepta los dos.
Toda la app usa formato es-BO (`MauiProgram.cs`), sin importar el idioma del celular.

**¿Qué mejorarían?**
HTTPS y publicar la API en la nube, *refresh tokens*, modo sin conexión con SQLite, notificaciones push para
las visitas, pruebas unitarias de los ViewModels y de las reglas de la API.

---

## 6. Guión de demostración (5 minutos)

1. **Login como Admin** (`adminmovil`) → mostrar el **ojo** de la contraseña.
2. **Inicio**: indicadores, gráficos de ventas por mes y comisiones por agente.
3. **Propiedades**: filtrar por tipo y estado → abrir una → foto, mapa y visitas.
4. **Registrar venta**: elegir propiedad (el monto se llena solo) y agente (**la comisión se calcula sola**) → guardar →
   volver a Propiedades y mostrar que quedó "Vendida".
5. **Mapa**: los colores por estado; acercar para ver los nombres.
6. **Cerrar sesión → entrar como agente** (`lucia` / `Agente123!`):
   - el Inicio muestra **sus** estadísticas,
   - en Ventas solo ve **las suyas** y no puede anularlas,
   - en su perfil **no puede cambiarse la comisión**.
7. **Swagger** (`http://localhost:5150/swagger`): `GET /api/Venta/Lista` **sin token → 401**.
8. Si hay tiempo: mostrar la app en el **celular**.

---

## 7. Archivos clave (para encontrar todo rápido)

| Quiero mostrar... | Archivo |
|---|---|
| Registro de servicios y páginas (DI) | `TerraNova/MauiProgram.cs` |
| Menú y rutas | `TerraNova/AppShell.xaml`, `Constantes/Rutas.cs` |
| Un ViewModel completo | `ViewModels/Ventas/VentaFormViewModel.cs` |
| Manejo de errores HTTP | `Services/Api/ServicioBase.cs` |
| Login y token | `Services/ServicioAutenticacion.cs`, API `Services/ServicioToken.cs` |
| Reglas de permisos | API `Services/Permisos.cs` |
| Transacción de venta | API `Controllers/VentaController.cs` → `Guardar` |
| Gráficos | `Controls/GraficoECharts.cs`, `Helpers/OpcionesGraficos.cs` |
| Mapa | `Mapas/MapaHelper.cs`, `Views/Propiedades/MapaPropiedadesPage.xaml.cs` |
| Estilos y colores | `Resources/Styles/TerraNova.xaml` |
| Creación de la base | API `Services/InicializadorBaseDatos.cs`, `BaseDatos/ScriptCompleto.sql` |
